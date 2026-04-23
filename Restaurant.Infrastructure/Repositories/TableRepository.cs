using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Data;


namespace Restaurant.Infrastructure.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly RestaurantDbContext _context;

        public TableRepository(RestaurantDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Table table)
        {
            await _context.Tables.AddAsync(table);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) { 
             _context.Tables.Remove(table);
              await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Table>> GetAllAsync()
        {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table> GetByIdAsync(int id)
        {
            return await _context.Tables.FindAsync(id);
        }

        public async Task UpdateAsync(Table table)
        {
            _context.Tables.Update(table);
            await _context.SaveChangesAsync();
        }
    }
}
