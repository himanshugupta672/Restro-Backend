using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class ChefService : IChefService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IChefRepository _chefRepository;

    public ChefService(
        IOrderRepository orderRepository,
        IChefRepository chefRepository)
    {
        _orderRepository = orderRepository;
        _chefRepository = chefRepository;
    }

    public async Task<List<Order>> GetAssignedOrders(int chefId)
    {
        var orders = await _orderRepository.GetAllAsync();

        return orders
            .Where(x => x.ChefId == chefId)
            .ToList();
    }

public async Task UpdateOrderStatus(int orderId, OrderStatus status)
{
    var order = await _orderRepository.GetByIdAsync(orderId);

    if (order == null)
        return;

    order.Status = status;

    if (status == OrderStatus.Completed)
    {
        var chef = await _chefRepository.GetByIdAsync(order.ChefId.Value);

        if (chef != null)
        {
            chef.IsAvailable = true;
            await _chefRepository.UpdateAsync(chef);
        }
    }

    await _orderRepository.UpdateAsync(order);
}
    public async Task AcceptOrder(int orderId, int chefId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null) return;

        order.Status = OrderStatus.Accepted;
        order.ChefId = chefId;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task RejectOrder(int orderId, int chefId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null) return;

        order.Status = OrderStatus.Pending;
        order.ChefId = null;

        await _orderRepository.UpdateAsync(order);
    }
}