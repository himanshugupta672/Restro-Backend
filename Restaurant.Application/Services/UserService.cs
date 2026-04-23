public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
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

    public async Task AddAsync(User user)
    {
        await _repository.AddAsync(user);
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
}