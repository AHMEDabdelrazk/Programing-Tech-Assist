namespace TechRoad.API.Services;

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        // If legacy unhashed plain text matches directly
        if (!IsHashed(passwordHash))
        {
            return password == passwordHash;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public bool IsHashed(string passwordOrHash)
    {
        if (string.IsNullOrWhiteSpace(passwordOrHash))
            return false;

        return passwordOrHash.StartsWith("$2a$") ||
               passwordOrHash.StartsWith("$2b$") ||
               passwordOrHash.StartsWith("$2y$");
    }
}
