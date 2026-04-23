using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        await _repository.AddAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        await _repository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}