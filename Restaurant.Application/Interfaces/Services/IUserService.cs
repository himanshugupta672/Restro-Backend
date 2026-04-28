using Restaurant.Application.DTOs;
using Restaurant.Domain.Entities;

public interface IUserService
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<List<User>> GetChefsAsync();

    Task RegisterChefAsync(CreateUserDto dto);
    Task CreateByAdminAsync(CreateUserDto dto);

    Task SetAvailable(int userId);

    Task SetBusy(int userId);

    Task SetOffline(int userId);
    Task<User?> GetNextAvailableChefAsync();
}