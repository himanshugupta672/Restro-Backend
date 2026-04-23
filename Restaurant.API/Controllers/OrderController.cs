using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _service.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        await _service.CreateAsync(order);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Order order)
    {
        await _service.UpdateAsync(order);
        return Ok();
    }

    [HttpGet("table/{tableId}")]
    public async Task<IActionResult> GetByTable(int tableId)
    {
        var orders = await _service.GetByTableId(tableId);
        return Ok(orders);
    }
}