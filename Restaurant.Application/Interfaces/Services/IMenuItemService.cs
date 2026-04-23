using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Services;

public interface IMenuItemService
{
    Task<IEnumerable<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByIdAsync(int id);
    Task AddAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(int id);
}