public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetChefsAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<List<User>> GetAvailableChefsAsync();
}