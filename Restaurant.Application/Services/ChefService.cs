using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class ChefService : IChefService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public ChefService(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    //public async Task<List<Order>> GetAssignedOrders(int chefId)
    //{
    //    var orders = await _orderRepository.GetAllAsync();

    //    return orders
    //        .Where(x => x.ChefId == chefId)
    //        .ToList();
    //}

    public async Task<List<Order>> GetAssignedOrders(int chefId)
    {
        var chef = await _userRepository.GetByIdAsync(chefId);
        var orders = await _orderRepository.GetAllAsync();

        if (chef == null) return new List<Order>();

        if (chef.Status == UserStatus.Available)
        {
            return orders
                .Where(x => x.Status == OrderStatus.Pending)
                .ToList();
        }

        return orders
            .Where(x => x.ChefId == chefId
                && x.Status != OrderStatus.Completed)
            .ToList();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    public async Task UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return;

        order.Status = status;

        if (status == OrderStatus.Completed && order.ChefId != null)
        {
            var chef = await _userRepository.GetByIdAsync(order.ChefId.Value);

            if (chef != null)
            {
                chef.Status = UserStatus.Available;
                await _userRepository.UpdateAsync(chef);
            }
        }

        await _orderRepository.UpdateAsync(order);
    }
    public async Task AcceptOrder(int orderId, int chefId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        var chef = await _userRepository.GetByIdAsync(chefId);

        if (order == null || chef == null) return;

        order.Status = OrderStatus.Accepted;
        order.ChefId = chefId;
        chef.Status = UserStatus.Busy;

        await _orderRepository.UpdateAsync(order);
        await _userRepository.UpdateAsync(chef);
    }

    public async Task RejectOrder(int orderId, int chefId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        var chef = await _userRepository.GetByIdAsync(chefId);

        if (order == null || chef == null) return;

        order.Status = OrderStatus.Pending;
        order.ChefId = null;
        chef.Status = UserStatus.Available;

        await _orderRepository.UpdateAsync(order);
        await _userRepository.UpdateAsync(chef);
    }


}
