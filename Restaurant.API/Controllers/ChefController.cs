using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Application.Services;
using Restaurant.Domain.Enums;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChefController : ControllerBase
{
    private readonly IChefService _service;

    public ChefController(IChefService service)
    {
        _service = service;
    }

    [HttpGet("{chefId}/orders")]
    public async Task<IActionResult> GetOrders(int chefId)
    {
        var orders = await _service.GetAssignedOrders(chefId);
        return Ok(orders);
    }

    [HttpPut("update-status")]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        await _service.UpdateOrderStatus(orderId, status);
        return Ok();
    }

    [HttpPut("accept-order")]
    public async Task<IActionResult> AcceptOrder(int orderId, int chefId)
    {
        await _service.AcceptOrder(orderId, chefId);
        return Ok();
    }

    [HttpPut("reject-order")]
    public async Task<IActionResult> RejectOrder(int orderId, int chefId)
    {
        await _service.RejectOrder(orderId, chefId);
        return Ok();
    }
}