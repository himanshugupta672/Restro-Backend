using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;
using System.Security.Claims;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Chef,Admin")] 
public class ChefController : ControllerBase
{
    private readonly IChefService _service;
    private readonly ITableService _tableService;

    public ChefController(IChefService service, ITableService tableService)
    {
        _service = service;
        _tableService = tableService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var chefId = GetUserId();
        var orders = await _service.GetAssignedOrders(chefId);
        var response = new List<object>();

        foreach (var order in orders)
        {
            response.Add(await ToOrderResponse(order));
        }

        return Ok(response);
    }

    [HttpPut("update-status")]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        var order = await _service.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(UserRole.Admin.ToString()) && order.ChefId != GetUserId())
        {
            return Forbid();
        }

        if (!IsValidStatusTransition(order.Status, status))
        {
            return BadRequest($"Cannot change order status from {order.Status} to {status}.");
        }

        await _service.UpdateOrderStatus(orderId, status);
        return Ok();
    }

    [HttpPut("accept-order")]
    public async Task<IActionResult> AcceptOrder(int orderId)
    {
        var chefId = GetUserId();
        var order = await _service.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.ChefId != null && order.ChefId != chefId)
        {
            return Forbid();
        }

        if (order.Status is not OrderStatus.Pending and not OrderStatus.Assigned)
        {
            return BadRequest("Only pending or assigned orders can be accepted.");
        }

        await _service.AcceptOrder(orderId, chefId);
        return Ok();
    }

    [HttpPut("reject-order")]
    public async Task<IActionResult> RejectOrder(int orderId)
    {
        var chefId = GetUserId();
        var order = await _service.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.ChefId != chefId)
        {
            return Forbid();
        }

        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
        {
            return BadRequest("Completed or cancelled orders cannot be rejected.");
        }

        await _service.RejectOrder(orderId, chefId);
        return Ok();
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
            OrderStatus.Accepted => next is OrderStatus.Preparing or OrderStatus.Cancelled,
            OrderStatus.Preparing => next is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => next is OrderStatus.Completed,
            _ => false
        };
    }
}
