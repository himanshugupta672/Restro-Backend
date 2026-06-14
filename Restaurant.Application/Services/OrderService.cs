using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IMenuItemRepository _menuRepository;

    public OrderService(
        IOrderRepository repository,
        IUserRepository userRepository,
        IMenuItemRepository menuRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _menuRepository = menuRepository;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        var pendingOrder = await _repository.GetPendingByTableIdAsync(order.TableId);
        var targetOrder = pendingOrder ?? order;
        decimal total = 0;

        foreach (var item in order.OrderItems)
        {
            var menuItem = await _menuRepository.GetByIdAsync(item.MenuItemId);

            if (menuItem == null)
                throw new Exception("Menu item not found");

            var existingItem = targetOrder.OrderItems
                .FirstOrDefault(x => x.MenuItemId == item.MenuItemId);

            if (existingItem == null)
            {
                item.Price = menuItem.Price;
                targetOrder.OrderItems.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
                existingItem.Price = menuItem.Price;
            }
        }

        total = targetOrder.OrderItems.Sum(item => item.Price * item.Quantity);

        targetOrder.TotalAmount = total;
        targetOrder.Status = OrderStatus.Pending;
        targetOrder.ChefId = null;

        if (pendingOrder == null)
        {
            targetOrder.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(targetOrder);
        }
        else
        {
            await _repository.UpdateAsync(targetOrder);
        }

        return targetOrder;
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

        var chef = await _userRepository.GetByIdAsync(chefId);
        if (chef != null)
        {
            chef.Status = UserStatus.Available;
            await _userRepository.UpdateAsync(chef);
        }

        var chefs = await _userRepository.GetAvailableChefsAsync();
        var nextChef = chefs.FirstOrDefault();

        if (nextChef != null)
        {
            order.ChefId = nextChef.Id;
            order.Status = OrderStatus.Assigned;

            nextChef.Status = UserStatus.Busy;
            nextChef.LastAssignedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(nextChef);
        }
        else
        {
            order.Status = OrderStatus.Pending;
            order.ChefId = null;
        }

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
