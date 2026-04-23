using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly RestaurantDbContext _context;

        public MenuItemRepository(RestaurantDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(MenuItem menuItem)
        {
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if(item != null)
            {
                _context.MenuItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems.ToListAsync();
        }

        public async Task<MenuItem> GetByIdAsync(int id)
        {
            return await _context.MenuItems.FindAsync(id);
        }

        public async Task UpdateAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
             await _context.SaveChangesAsync();
        }
    }
}
