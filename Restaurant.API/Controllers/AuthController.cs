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
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        ITokenBlacklistService tokenBlacklistService,
        IPasswordHashService passwordHashService,
        IRefreshTokenService refreshTokenService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _tokenBlacklistService = tokenBlacklistService;
        _passwordHashService = passwordHashService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHashService.VerifyPassword(request.Password, user.Password))
        {
            return Unauthorized();
        }

        if (!_passwordHashService.IsPasswordHash(user.Password))
        {
            user.Password = _passwordHashService.HashPassword(request.Password);
            await _userRepository.UpdateAsync(user);
        }

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        SetRefreshTokenCookie(refreshToken);

        return Ok(new
        {
            accessToken,
            token = accessToken,
            role = user.Role,
            userId = user.Id
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
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

        var accessToken = _jwtService.GenerateToken(storedToken.User);

        return Ok(new
        {
            accessToken,
            token = accessToken,
            role = storedToken.User.Role,
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

        return Ok(new
        {
            message = "Logged out successfully"
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

    private int GetRefreshTokenDurationInDays()
    {
        return int.TryParse(_configuration["Jwt:RefreshTokenDurationInDays"], out var days)
            ? days
            : 7;
    }
}
