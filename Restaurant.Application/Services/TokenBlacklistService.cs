using System.Collections.Concurrent;
using Restaurant.Application.Interfaces.Services;

namespace Restaurant.Application.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

    public Task BlacklistTokenAsync(string token, DateTime expiresAtUtc)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _blacklistedTokens[token] = expiresAtUtc;
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (!_blacklistedTokens.TryGetValue(token, out var expiresAtUtc))
        {
            return Task.FromResult(false);
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            _blacklistedTokens.TryRemove(token, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
