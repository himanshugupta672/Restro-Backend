using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;
using System.Security.Claims;

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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (User.Identity?.IsAuthenticated == true &&
            roleClaim == UserRole.Customer.ToString() &&
            !string.IsNullOrEmpty(userIdClaim) &&
            int.TryParse(userIdClaim, out var parsedId))
        {
            customerId = parsedId;
        }

        var order = new Order
        {
            TableId = table.Id,
            CustomerId = customerId,
            SpecialInstructions = dto.SpecialInstructions,
            Status = OrderStatus.Pending,
            OrderItems = dto.Items.Select(x => new OrderItem
            {
                MenuItemId = x.MenuItemId,
                Quantity = x.Quantity
            }).ToList()
        };

        Order savedOrder;
        try
        {
            savedOrder = await _service.CreateAsync(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

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
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderDto? dto,
        [FromQuery] OrderStatus? status)
    {
        var order = await _service.GetByIdAsync(id);
        if (order == null) return NotFound();

        var requestedStatus = dto?.Status ?? status;
        if (requestedStatus == null)
        {
            return BadRequest("Order status is required.");
        }

        if (!IsValidStatusTransition(order.Status, requestedStatus.Value))
        {
            return BadRequest($"Cannot change order status from {order.Status} to {requestedStatus.Value}.");
        }

        if (User.IsInRole(UserRole.Chef.ToString()))
        {
            var chefIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(chefIdClaim, out var chefId) || order.ChefId != chefId)
            {
                return Forbid();
            }
        }

        if (User.IsInRole(UserRole.Admin.ToString()))
        {
            try
            {
                await _service.UpdateStatusAndChefAsync(id, requestedStatus.Value, dto?.ChefId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return Ok("Order updated");
        }

        order.Status = requestedStatus.Value;
        await _service.UpdateAsync(order);

        return Ok("Status updated");
    }

    [Authorize(Roles = "Admin,Chef")]
    [HttpGet("table/{tableId}")]
    public async Task<IActionResult> GetByTable(int tableId)
    {
        return Ok(await _service.GetByTableId(tableId));
    }

    [Authorize]
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var orders = await _service.GetByCustomerIdAsync(customerId);
        var response = new List<CustomerOrderResponseDto>();

        foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
        {
            var table = await _tableService.GetByIdAsync(order.TableId);
            response.Add(await ToCustomerResponse(order, table?.TableNumber ?? order.TableId));
        }

        return Ok(response);
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
            EstimatedTimeMinutes = order.EstimatedReadyMinutes,
            EstimatedReadyAt = order.EstimatedReadyMinutes.HasValue
                ? order.CreatedAt.AddMinutes(order.EstimatedReadyMinutes.Value).ToString("o")
                : null,
            SpecialInstructions = order.SpecialInstructions,
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

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next)
        {
            return true;
        }

        return current switch
        {
            OrderStatus.Pending => next is OrderStatus.Assigned or OrderStatus.Accepted or OrderStatus.Cancelled,
            OrderStatus.Assigned => next is OrderStatus.Accepted or OrderStatus.Pending or OrderStatus.Cancelled,
            OrderStatus.Accepted => next is OrderStatus.Assigned or OrderStatus.Preparing or OrderStatus.Cancelled,
            OrderStatus.Preparing => next is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => next is OrderStatus.Completed,
            OrderStatus.Completed => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}
