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
        return Ok(await _service.GetAllAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("chefs")]
    public async Task<IActionResult> GetChefs()
    {
        return Ok(await _service.GetChefsAsync());
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
}