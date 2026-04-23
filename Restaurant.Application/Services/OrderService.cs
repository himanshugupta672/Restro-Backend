using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IUserRepository _userRepository;

    public OrderService(
        IOrderRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task CreateAsync(Order order)
    {
        var chefs = await _userRepository.GetAvailableChefsAsync();
        var chef = chefs.FirstOrDefault();

        if (chef != null)
        {
            order.ChefId = chef.Id;
            order.Status = OrderStatus.Assigned;

            chef.Status = UserStatus.Busy;
            chef.LastAssignedAt = DateTime.Now;
            await _userRepository.UpdateAsync(chef);
        }
        else
        {
            order.Status = OrderStatus.Pending;
        }

        await _repository.AddAsync(order);
    }

    public async Task UpdateAsync(Order order)
    {
        await _repository.UpdateAsync(order);
    }
    public async Task<List<Order>> GetByTableId(int tableId)
    {
        var orders = await _repository.GetAllAsync();

        return orders
            .Where(x => x.TableId == tableId)
            .ToList();
    }
    public async Task AcceptOrder(int orderId, int chefId)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order == null) return;

        order.Status = OrderStatus.Accepted;
        order.ChefId = chefId;

        await _repository.UpdateAsync(order);
    }
    public async Task RejectOrder(int orderId, int chefId)
    {
        var order = await _repository.GetByIdAsync(orderId);

        if (order == null) return;

        order.Status = OrderStatus.Pending;
        order.ChefId = null;

        await _repository.UpdateAsync(order);
    }
    public async Task<List<Order>> GetAssignedOrders(int chefId)
    {
        var orders = await _repository.GetAllAsync();

        return orders
            .Where(x => x.ChefId == chefId
            && x.Status == OrderStatus.Assigned)
            .ToList();
    }
}