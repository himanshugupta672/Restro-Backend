using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly ITableService _tableService;

    public OrderController(IOrderService service, ITableService tableService)
    {
        _service = service;
        _tableService = tableService;
    }


    [Authorize(Roles = "Admin,Chef")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [Authorize(Roles = "Admin,Chef")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(string token, CreateOrderDto dto)
    {
        var table = await _tableService.GetByTokenAsync(token);

        if (table == null)
            return BadRequest("Invalid or inactive table");

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.Pending,
            OrderItems = dto.Items.Select(x => new OrderItem
            {
                MenuItemId = x.MenuItemId,
                Quantity = x.Quantity
            }).ToList()
        };

        await _service.CreateAsync(order);

        return Ok("Order placed successfully");
    }

    [Authorize(Roles = "Admin,Chef")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _service.GetByIdAsync(id);
        if (order == null) return NotFound();

        order.Status = status;

        await _service.UpdateAsync(order);

        return Ok("Status updated");
    }

    [Authorize(Roles = "Admin,Chef")]
    [HttpGet("table/{tableId}")]
    public async Task<IActionResult> GetByTable(int tableId)
    {
        return Ok(await _service.GetByTableId(tableId));
    }
}