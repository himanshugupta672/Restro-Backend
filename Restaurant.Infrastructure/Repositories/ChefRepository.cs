using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories;

public class ChefRepository : IChefRepository
{
    private readonly RestaurantDbContext _context;

    public ChefRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<List<Chef>> GetAvailableChefsAsync()
    {
        return await _context.Chefs
            .Where(x => x.IsAvailable)
            .ToListAsync();
    }

    public async Task<Chef?> GetByIdAsync(int id)
    {
        return await _context.Chefs.FindAsync(id);
    }

    public async Task UpdateAsync(Chef chef)
    {
        _context.Chefs.Update(chef);
        await _context.SaveChangesAsync();
    }
}