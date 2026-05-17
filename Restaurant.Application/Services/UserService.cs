using Restaurant.Application.DTOs;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHashService _passwordHashService;

    public UserService(
        IUserRepository repository,
        IPasswordHashService passwordHashService)
    {
        _repository = repository;
        _passwordHashService = passwordHashService;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<User>> GetChefsAsync()
    {
        return await _repository.GetChefsAsync();
    }

    public async Task SetAvailable(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Available;
        await _repository.UpdateAsync(user);
    }

    public async Task SetBusy(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Busy;
        await _repository.UpdateAsync(user);
    }

    public async Task SetOffline(int userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Offline;
        await _repository.UpdateAsync(user);
    }
    public async Task<User?> GetNextAvailableChefAsync()
    {
        var chefs = await _repository.GetAvailableChefsAsync();

        return chefs.FirstOrDefault();
    }
    public async Task RegisterChefAsync(CreateUserDto dto)
    {
        var existing = await _repository.GetByEmailAsync(dto.Email);

        if (existing != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = _passwordHashService.HashPassword(dto.Password),
            Role = UserRole.Chef,
            Status = UserStatus.Available,
            LastAssignedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user); 
    }

    public async Task CreateByAdminAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = _passwordHashService.HashPassword(dto.Password),

            Role = (UserRole)dto.Role, // admin decides
            Status = UserStatus.Available,
            LastAssignedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user);
    }
}
