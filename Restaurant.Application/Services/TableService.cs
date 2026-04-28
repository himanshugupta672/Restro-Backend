using Restaurant.Application.Interfaces.Repositories;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _repository;

    public TableService(ITableRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Table>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Table?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(Table table)
    {
        table.Token = Guid.NewGuid().ToString();
        await _repository.AddAsync(table);
    }

    public async Task UpdateAsync(Table table)
    {
        await _repository.UpdateAsync(table);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
    public async Task<Table?> GetByTokenAsync(string token)
    {
        var tables = await _repository.GetAllAsync();

        return tables.FirstOrDefault(x => x.Token == token && x.IsActive);
    }
}