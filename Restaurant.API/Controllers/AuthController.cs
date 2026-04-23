using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthController(
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var users = await _userRepository.GetAllAsync();

        var user = users.FirstOrDefault(x =>
            x.Email == request.Email &&
            x.Password == request.Password);

        if (user == null)
            return Unauthorized();

        var token = _jwtService.GenerateToken(user);

        return Ok(new
        {
            token,
            role = user.Role,
            userId = user.Id
        });
    }
}