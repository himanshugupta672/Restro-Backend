using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;

    public MenuItemService(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(MenuItem item)
    {
        await _repository.AddAsync(item);
    }

    public async Task UpdateAsync(MenuItem item)
    {
        await _repository.UpdateAsync(item);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}