using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RestaurantDbContext _context;

    public OrderRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(x => x.OrderItems)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}