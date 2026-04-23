using Restaurant.Domain.Entities;

namespace Restaurant.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}