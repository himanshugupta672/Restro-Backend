using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Repositories;

public interface IChefRepository
{
    Task<List<Chef>> GetAvailableChefsAsync();

    Task<Chef?> GetByIdAsync(int id);

    Task UpdateAsync(Chef chef);
}