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
    [HttpGet("chefs/available")]
    public async Task<IActionResult> GetAvailableChefs()
    {
        var chefs = await _service.GetAvailableChefsAsync();
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
            // Security override: self-registered users are always created as a Chef
            dto.Role = (int)UserRole.Chef;
            await _service.RegisterChefAsync(dto);
            return Ok("Chef registered successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
            {
                return NotFound("User not found");
            }
            return Ok("User updated successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var details = await _service.GetCustomerDetailsAsync();
        return Ok(details);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _service.ToggleUserActiveStatusAsync(id);
        if (!result)
        {
            return NotFound("User not found");
        }
        return Ok("Active status toggled successfully");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await _service.DeleteAsync(id);

        if (!isDeleted)
        {
            return NotFound("User not found");
        }

        return Ok("User deleted successfully");
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
            IsActive = user.IsActive,
            LastAssignedAt = user.LastAssignedAt
        };
    }
}
