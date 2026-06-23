public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetChefsAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<List<User>> GetAvailableChefsAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetCustomersAsync();
    Task<User?> GetByPhoneAsync(string phone);
}
