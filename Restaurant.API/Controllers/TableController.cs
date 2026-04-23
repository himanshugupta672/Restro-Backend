using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Table table)
    {
        await _service.AddAsync(table);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok();
    }
}