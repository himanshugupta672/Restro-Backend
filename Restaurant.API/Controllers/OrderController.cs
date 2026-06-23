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
    private readonly IMenuItemService _menuItemService;

    public OrderController(
        IOrderService service,
        ITableService tableService,
        IMenuItemService menuItemService)
    {
        _service = service;
        _tableService = tableService;
        _menuItemService = menuItemService;
    }


    [Authorize(Roles = "Admin,Chef")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _service.GetAllAsync();
        var response = new List<object>();

        foreach (var order in orders)
        {
            response.Add(await ToOrderResponse(order));
        }

        return Ok(response);
    }

    [Authorize(Roles = "Admin,Chef")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(await ToOrderResponse(order));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        if (dto.TableNumber <= 0)
        {
            return BadRequest("A valid table number is required.");
        }

        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest("At least one order item is required.");
        }

        var table = await _tableService.GetByTableNumberAsync(dto.TableNumber);

        if (table == null)
            return BadRequest("Invalid or inactive table number.");

        int? customerId = null;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
        {
            customerId = parsedId;
        }

        var order = new Order
        {
            TableId = table.Id,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            OrderItems = dto.Items.Select(x => new OrderItem
            {
                MenuItemId = x.MenuItemId,
                Quantity = x.Quantity
            }).ToList()
        };

        var savedOrder = await _service.CreateAsync(order);

        return Ok(await ToCustomerResponse(savedOrder, table.TableNumber));
    }

    [AllowAnonymous]
    [HttpGet("{id}/track")]
    public async Task<IActionResult> Track(int id, int tableNumber)
    {
        if (tableNumber <= 0)
        {
            return BadRequest("A valid table number is required.");
        }

        var table = await _tableService.GetByTableNumberAsync(tableNumber);
        if (table == null)
        {
            return NotFound("The table number is invalid or inactive.");
        }

        var order = await _service.GetByIdAsync(id);
        if (order == null || order.TableId != table.Id)
        {
            return NotFound("Order not found for this table.");
        }

        return Ok(await ToCustomerResponse(order, table.TableNumber));
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

    private async Task<CustomerOrderResponseDto> ToCustomerResponse(
        Order order,
        int tableNumber)
    {
        var items = new List<CustomerOrderItemResponseDto>();

        foreach (var orderItem in order.OrderItems)
        {
            var menuItem = await _menuItemService.GetByIdAsync(orderItem.MenuItemId);

            items.Add(new CustomerOrderItemResponseDto
            {
                MenuItemId = orderItem.MenuItemId,
                Name = menuItem?.Name ?? $"Menu item #{orderItem.MenuItemId}",
                Quantity = orderItem.Quantity,
                Price = orderItem.Price,
                LineTotal = orderItem.Price * orderItem.Quantity
            });
        }

        return new CustomerOrderResponseDto
        {
            OrderId = order.Id,
            TableId = order.TableId,
            TableNumber = tableNumber,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = items
        };
    }

    private async Task<object> ToOrderResponse(Order order)
    {
        var table = await _tableService.GetByIdAsync(order.TableId);

        return new
        {
            order.Id,
            order.TableId,
            TableNumber = table?.TableNumber ?? order.TableId,
            order.Status,
            order.ChefId,
            order.CreatedAt,
            order.TotalAmount,
            order.OrderItems
        };
    }
}
