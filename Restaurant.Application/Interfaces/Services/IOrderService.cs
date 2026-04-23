using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task CreateAsync(Order order);

    Task UpdateAsync(Order order);
    Task<List<Order>> GetByTableId(int tableId);
    Task AcceptOrder(int orderId, int chefId);

    Task RejectOrder(int orderId, int chefId);

    Task<List<Order>> GetAssignedOrders(int chefId);
}