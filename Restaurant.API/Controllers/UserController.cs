using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _service.GetAllAsync();
        return Ok(users.Select(ToResponseDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("chefs")]
    public async Task<IActionResult> GetChefs()
    {
        var chefs = await _service.GetChefsAsync();
        return Ok(chefs.Select(ToResponseDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        await _service.CreateByAdminAsync(dto);
        return Ok("User created successfully");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        try
        {
            await _service.RegisterChefAsync(dto);
            return Ok("Chef registered successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static UserResponseDto ToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            LastAssignedAt = user.LastAssignedAt
        };
    }
}
