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
        if (order.OrderItems.Count == 0)
        {
            throw new ArgumentException("At least one order item is required.");
        }

        var validatedItems = new List<OrderItem>();

        foreach (var item in order.OrderItems)
        {
            if (item.Quantity <= 0 || item.Quantity > 100)
            {
                throw new ArgumentException("Order item quantity must be between 1 and 100.");
            }

            var menuItem = await _menuRepository.GetByIdAsync(item.MenuItemId);

            if (menuItem == null)
                throw new ArgumentException($"Menu item #{item.MenuItemId} was not found.");

            if (!menuItem.IsAvailable)
            {
                throw new ArgumentException($"{menuItem.Name} is not available.");
            }

            var existingItem = validatedItems.FirstOrDefault(x => x.MenuItemId == item.MenuItemId);
            if (existingItem == null)
            {
                validatedItems.Add(new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Price = menuItem.Price
                });
                continue;
            }

            existingItem.Quantity += item.Quantity;
        }

        order.OrderItems = validatedItems;
        order.TotalAmount = order.OrderItems.Sum(item => item.Price * item.Quantity);
        order.Status = OrderStatus.Pending;
        order.ChefId = null;
        order.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(order);

        return order;
    }
    public async Task UpdateAsync(Order order)
    {
        await _repository.UpdateAsync(order);
    }

    public async Task<Order> UpdateStatusAndChefAsync(int orderId, OrderStatus status, int? chefId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (!IsValidStatusTransition(order.Status, status))
        {
            throw new ArgumentException($"Cannot change order status from {order.Status} to {status}.");
        }

        User? selectedChef = null;
        if (chefId.HasValue)
        {
            selectedChef = await _userRepository.GetByIdAsync(chefId.Value);
            if (selectedChef == null ||
                selectedChef.Role != UserRole.Chef)
            {
                throw new ArgumentException("Selected chef does not exist.");
            }

            if (!selectedChef.IsActive &&
                (order.ChefId != selectedChef.Id || RequiresChef(status)))
            {
                throw new ArgumentException("Selected chef is inactive. Choose an active available chef.");
            }

            if (selectedChef.Status != UserStatus.Available &&
                order.ChefId != selectedChef.Id)
            {
                throw new ArgumentException("Selected chef is not currently available.");
            }
        }

        if (RequiresChef(status) && !chefId.HasValue)
        {
            throw new ArgumentException($"A chef is required when moving an order to {status}.");
        }

        if (order.ChefId.HasValue && order.ChefId != chefId)
        {
            var previousChef = await _userRepository.GetByIdAsync(order.ChefId.Value);
            if (previousChef != null && previousChef.IsActive)
            {
                previousChef.Status = UserStatus.Available;
                await _userRepository.UpdateAsync(previousChef);
            }
        }

        order.Status = status;
        order.ChefId = chefId;

        if (selectedChef != null && selectedChef.IsActive && !IsTerminalStatus(status))
        {
            selectedChef.Status = UserStatus.Busy;
            selectedChef.LastAssignedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(selectedChef);
        }

        if (selectedChef != null && selectedChef.IsActive && IsTerminalStatus(status))
        {
            selectedChef.Status = UserStatus.Available;
            await _userRepository.UpdateAsync(selectedChef);
        }

        await _repository.UpdateAsync(order);
        return order;
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

    private static bool RequiresChef(OrderStatus status)
    {
        return status is OrderStatus.Assigned
            or OrderStatus.Accepted
            or OrderStatus.Preparing
            or OrderStatus.Ready;
    }

    private static bool IsTerminalStatus(OrderStatus status)
    {
        return status is OrderStatus.Completed or OrderStatus.Cancelled;
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
