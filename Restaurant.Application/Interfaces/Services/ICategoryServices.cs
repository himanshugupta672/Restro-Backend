using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task DeleteAsync(int id);
        Task UpdateAsync(Category category);
    }
}
