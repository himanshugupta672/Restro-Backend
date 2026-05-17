namespace Restaurant.Application.Interfaces.Services;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string token, DateTime expiresAtUtc);

    Task<bool> IsTokenBlacklistedAsync(string token);
}
