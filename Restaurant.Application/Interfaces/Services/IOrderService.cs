using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Interfaces.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task<Order> CreateAsync(Order order);

    Task UpdateAsync(Order order);

    Task<Order> UpdateStatusAndChefAsync(int orderId, OrderStatus status, int? chefId);

    Task<List<Order>> GetByTableId(int tableId);

    Task<List<Order>> GetByCustomerIdAsync(int customerId);

    Task AcceptOrder(int orderId, int chefId);

    Task RejectOrder(int orderId, int chefId);

    Task<List<Order>> GetAssignedOrders(int chefId);
}
