namespace TechRoad.API.Services;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    bool IsHashed(string passwordOrHash);
}
