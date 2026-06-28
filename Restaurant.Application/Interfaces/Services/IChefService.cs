using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Interfaces.Services;

public interface IChefService
{
    Task<List<Order>> GetAssignedOrders(int chefId);

    Task<Order?> GetOrderByIdAsync(int orderId);

    Task UpdateOrderStatus(int orderId, OrderStatus status);
    Task AcceptOrder(int orderId, int chefId);

    Task RejectOrder(int orderId, int chefId);
}
