using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController : ControllerBase
{
    private readonly ITableService _service;

    public TableController(ITableService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [AllowAnonymous]
    [HttpGet("session")]
    public async Task<IActionResult> GetSession(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("Table token is required.");
        }

        var table = await _service.GetByTokenAsync(token);

        if (table == null)
        {
            return NotFound("The table session is invalid or inactive.");
        }

        return Ok(new
        {
            tableId = table.Id,
            tableNumber = table.TableNumber
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTableDto dto)
    {
        var table = new Table
        {
            TableNumber = dto.TableNumber,
            IsActive = dto.IsActive,
            Token = Guid.NewGuid().ToString() 
        };

        await _service.AddAsync(table);

        return Ok(table);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok("Deleted successfully");
    }
}
