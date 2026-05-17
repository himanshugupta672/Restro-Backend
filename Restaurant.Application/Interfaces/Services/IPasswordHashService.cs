public interface IPasswordHashService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string storedPassword);

    bool IsPasswordHash(string storedPassword);
}
