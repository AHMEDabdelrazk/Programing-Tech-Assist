using TechRoad.API.Models;
using TechRoad.API.Models.DTOs;

namespace TechRoad.API.Services;

public class AuthService : IAuthService
{
    private readonly IJsonStorageService _storage;
    private readonly IPasswordHasherService _hasher;
    private readonly ITokenService _tokenService;
    private readonly IProgressService _progressService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IJsonStorageService storage,
        IPasswordHasherService hasher,
        ITokenService tokenService,
        IProgressService progressService,
        ILogger<AuthService> logger)
    {
        _storage = storage;
        _hasher = hasher;
        _tokenService = tokenService;
        _progressService = progressService;
        _logger = logger;
    }

    public (bool Success, string Message, AuthResponseDto? Response) Register(RegisterRequestDto request)
    {
        var existingEmail = _storage.GetUserByEmail(request.Email);
        if (existingEmail != null)
        {
            return (false, "An account with this email already exists.", null);
        }

        var existingUsername = _storage.GetUserByUsername(request.Username);
        if (existingUsername != null)
        {
            return (false, "This username is already taken.", null);
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _hasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _storage.SaveUser(user);

        var token = _tokenService.GenerateToken(user, out var expiresAt);

        _logger.LogInformation("New user registered: {Username} ({Email})", user.Username, user.Email);

        return (true, "User created successfully.", new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = expiresAt
        });
    }

    public (bool Success, string Message, AuthResponseDto? Response) Login(LoginRequestDto request)
    {
        var user = _storage.GetUserByEmail(request.Email);
        if (user == null)
        {
            return (false, "Invalid email or password.", null);
        }

        var isPasswordValid = _hasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return (false, "Invalid email or password.", null);
        }

        // If user logged in with legacy plaintext password, upgrade it to BCrypt hash
        if (!_hasher.IsHashed(user.PasswordHash))
        {
            user.PasswordHash = _hasher.HashPassword(request.Password);
            _storage.SaveUser(user);
            _logger.LogInformation("Upgraded legacy password to BCrypt for user {Username}", user.Username);
        }

        var token = _tokenService.GenerateToken(user, out var expiresAt);

        _logger.LogInformation("User logged in: {Username}", user.Username);

        return (true, "Login successful.", new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = expiresAt
        });
    }

    public UserProfileDto? GetProfile(string username)
    {
        var user = _storage.GetUserByUsername(username);
        if (user == null) return null;

        var stats = _progressService.GetStatsForUser(username);

        return new UserProfileDto
        {
            Username = user.Username,
            Email = user.Email,
            RoadmapsStarted = stats.TechnologiesTracked,
            AverageProgress = stats.AverageScore,
            TotalCompletedLevels = stats.TotalLevelsCompleted,
            TotalProjectsBuilt = stats.TotalProjects
        };
    }
}
