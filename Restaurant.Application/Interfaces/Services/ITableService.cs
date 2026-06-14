using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Services;

public interface ITableService
{
    Task<IEnumerable<Table>> GetAllAsync();
    Task<Table?> GetByIdAsync(int id);
    Task AddAsync(Table table);
    Task UpdateAsync(Table table);
    Task DeleteAsync(int id);
    Task<Table?> GetByTokenAsync(string token);
    Task<Table?> GetByTableNumberAsync(int tableNumber);
}
