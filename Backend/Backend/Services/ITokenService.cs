using TechRoad.API.Models;

namespace TechRoad.API.Services;

public interface ITokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}
