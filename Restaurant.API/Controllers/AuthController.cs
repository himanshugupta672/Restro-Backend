using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)> OtpCache = 
        new System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)>();

    private const string RefreshTokenCookieName = "refreshToken";
    private const string CsrfTokenCookieName = "csrfToken";
    private const string CsrfTokenHeaderName = "X-CSRF-TOKEN";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        ITokenBlacklistService tokenBlacklistService,
        IPasswordHashService passwordHashService,
        IRefreshTokenService refreshTokenService,
        IUserService userService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _tokenBlacklistService = tokenBlacklistService;
        _passwordHashService = passwordHashService;
        _refreshTokenService = refreshTokenService;
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !_passwordHashService.VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized();
            }

            if (!user.IsActive)
            {
                return Unauthorized("Your account has been deactivated.");
            }

            if (!_passwordHashService.IsPasswordHash(user.Password))
            {
                user.Password = _passwordHashService.HashPassword(request.Password);
                await _userRepository.UpdateAsync(user);
            }

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            SetRefreshTokenCookie(refreshToken);
            SetCsrfTokenCookie();

            return Ok(new
            {
                accessToken,
                token = accessToken,
                role = user.Role.ToString(),
                userId = user.Id
            });
        }
         catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        if (!IsValidCsrfToken())
        {
            return Forbid();
        }

        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var tokenHash = _refreshTokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken == null ||
            storedToken.RevokedAtUtc != null ||
            storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            ClearRefreshTokenCookie();
            return Unauthorized();
        }

        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _refreshTokenService.HashRefreshToken(newRefreshToken);

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        await SaveRefreshTokenAsync(storedToken.UserId, newRefreshTokenHash);
        SetRefreshTokenCookie(newRefreshToken);
        SetCsrfTokenCookie();

        var accessToken = _jwtService.GenerateToken(storedToken.User);

        return Ok(new
        {
            accessToken,
            token = accessToken,
            role = storedToken.User.Role.ToString(),
            userId = storedToken.User.Id
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Bearer token is required.");
        }

        var accessToken = authorizationHeader["Bearer ".Length..].Trim();
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        await _tokenBlacklistService.BlacklistTokenAsync(accessToken, jwtToken.ValidTo);
        await RevokeRefreshTokenFromCookieAsync();
        ClearRefreshTokenCookie();
        ClearCsrfTokenCookie();

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        try
        {
            var isUpdated = await _userService.ResetPasswordAsync(dto);

            if (!isUpdated)
            {
                return NotFound("User not found");
            }

            return Ok("Password updated successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("customer/register")]
    public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterCustomerAsync(dto);

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            SetRefreshTokenCookie(refreshToken);
            SetCsrfTokenCookie();

            return Ok(new
            {
                accessToken,
                token = accessToken,
                role = user.Role.ToString(),
                userId = user.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp(SendOtpRequestDto dto)
    {
        string? identifier = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email.Trim().ToLower() : dto.PhoneNumber?.Trim();
        if (string.IsNullOrEmpty(identifier))
        {
            return BadRequest("Email or Phone Number is required.");
        }

        User? user = null;
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            user = await _userRepository.GetByEmailAsync(dto.Email);
        }
        else if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneAsync(dto.PhoneNumber);
        }

        if (user == null)
        {
            return NotFound("User not registered. Please register first.");
        }

        if (!user.IsActive)
        {
            return Unauthorized("Your account has been deactivated.");
        }

        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        OtpCache[identifier] = (otp, expiresAt);

        Console.WriteLine($"[DEMO OTP] Sent verification code {otp} to {identifier}");

        return Ok(new
        {
            message = $"OTP sent successfully (for testing, code is: {otp})",
            otp = otp
        });
    }

    [AllowAnonymous]
    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto dto)
    {
        string? identifier = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email.Trim().ToLower() : dto.PhoneNumber?.Trim();
        if (string.IsNullOrEmpty(identifier))
        {
            return BadRequest("Email or Phone Number is required.");
        }

        if (!OtpCache.TryGetValue(identifier, out var cachedData) || cachedData.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("OTP has expired or was not requested.");
        }

        if (cachedData.Otp != dto.Otp)
        {
            return BadRequest("Invalid OTP verification code.");
        }

        OtpCache.TryRemove(identifier, out _);

        User? user = null;
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            user = await _userRepository.GetByEmailAsync(dto.Email);
        }
        else if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneAsync(dto.PhoneNumber);
        }

        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (!user.IsActive)
        {
            return Unauthorized("Your account has been deactivated.");
        }

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        SetRefreshTokenCookie(refreshToken);
        SetCsrfTokenCookie();

        return Ok(new
        {
            accessToken,
            token = accessToken,
            role = user.Role.ToString(),
            userId = user.Id
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    private async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var tokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

        await SaveRefreshTokenAsync(userId, tokenHash);

        return refreshToken;
    }

    private async Task SaveRefreshTokenAsync(int userId, string tokenHash)
    {
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenDurationInDays())
        });
    }

    private async Task RevokeRefreshTokenFromCookieAsync()
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _refreshTokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken == null || storedToken.RevokedAtUtc != null)
        {
            return;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(storedToken);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            GetRefreshTokenCookieOptions());
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            GetRefreshTokenCookieOptions());
    }

    private void SetCsrfTokenCookie()
    {
        Response.Cookies.Append(
            CsrfTokenCookieName,
            Guid.NewGuid().ToString("N"),
            GetCsrfTokenCookieOptions());
    }

    private void ClearCsrfTokenCookie()
    {
        Response.Cookies.Delete(
            CsrfTokenCookieName,
            GetCsrfTokenCookieOptions());
    }

    private bool IsValidCsrfToken()
    {
        if (!Request.Cookies.TryGetValue(CsrfTokenCookieName, out var csrfCookie) ||
            string.IsNullOrWhiteSpace(csrfCookie))
        {
            return false;
        }

        var csrfHeader = Request.Headers[CsrfTokenHeaderName].ToString();

        return !string.IsNullOrWhiteSpace(csrfHeader) &&
            string.Equals(csrfCookie, csrfHeader, StringComparison.Ordinal);
    }

    private CookieOptions GetRefreshTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(GetRefreshTokenDurationInDays())
        };
    }

    private CookieOptions GetCsrfTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(GetRefreshTokenDurationInDays())
        };
    }

    private int GetRefreshTokenDurationInDays()
    {
        return int.TryParse(_configuration["Jwt:RefreshTokenDurationInDays"], out var days)
            ? days
            : 7;
    }
}
