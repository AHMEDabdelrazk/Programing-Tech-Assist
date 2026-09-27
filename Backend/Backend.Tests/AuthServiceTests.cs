using Microsoft.Extensions.Logging;
using Moq;
using TechRoad.API.Models;
using TechRoad.API.Models.DTOs;
using TechRoad.API.Services;
using Xunit;

namespace TechRoad.Tests;

public class AuthServiceTests
{
    private readonly Mock<IJsonStorageService> _mockStorage = new();
    private readonly Mock<IPasswordHasherService> _mockHasher = new();
    private readonly Mock<ITokenService> _mockToken = new();
    private readonly Mock<IProgressService> _mockProgress = new();
    private readonly Mock<ILogger<AuthService>> _mockLogger = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _mockStorage.Object,
            _mockHasher.Object,
            _mockToken.Object,
            _mockProgress.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void Register_WhenEmailAlreadyExists_ShouldFail()
    {
        var request = new RegisterRequestDto
        {
            Username = "NewUser",
            Email = "existing@techroad.dev",
            Password = "Password123!"
        };

        _mockStorage.Setup(s => s.GetUserByEmail(request.Email))
            .Returns(new User { Email = request.Email });

        var (success, message, response) = _service.Register(request);

        Assert.False(success);
        Assert.Contains("email already exists", message);
        Assert.Null(response);
    }

    [Fact]
    public void Register_WhenSuccessful_ShouldHashPasswordAndReturnToken()
    {
        var request = new RegisterRequestDto
        {
            Username = "UniqueUser",
            Email = "unique@techroad.dev",
            Password = "Password123!"
        };

        _mockStorage.Setup(s => s.GetUserByEmail(request.Email)).Returns((User?)null);
        _mockStorage.Setup(s => s.GetUserByUsername(request.Username)).Returns((User?)null);
        _mockHasher.Setup(h => h.HashPassword(request.Password)).Returns("$2a$11$HashedPassword");

        var testExpires = DateTime.UtcNow.AddHours(24);
        _mockToken.Setup(t => t.GenerateToken(It.IsAny<User>(), out testExpires))
            .Returns("mocked.jwt.token");

        var (success, message, response) = _service.Register(request);

        Assert.True(success);
        Assert.NotNull(response);
        Assert.Equal("mocked.jwt.token", response.Token);
        Assert.Equal(request.Username, response.Username);
        _mockStorage.Verify(s => s.SaveUser(It.Is<User>(u => 
            u.Username == request.Username && 
            u.PasswordHash == "$2a$11$HashedPassword")), Times.Once);
    }

    [Fact]
    public void Login_WhenUserNotFound_ShouldFail()
    {
        var request = new LoginRequestDto
        {
            Email = "missing@techroad.dev",
            Password = "Password123!"
        };

        _mockStorage.Setup(s => s.GetUserByEmail(request.Email)).Returns((User?)null);

        var (success, message, response) = _service.Login(request);

        Assert.False(success);
        Assert.Contains("Invalid email or password", message);
        Assert.Null(response);
    }

    [Fact]
    public void Login_WhenPasswordInvalid_ShouldFail()
    {
        var request = new LoginRequestDto
        {
            Email = "user@techroad.dev",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Username = "UserOne",
            Email = request.Email,
            PasswordHash = "$2a$11$HashedPassword"
        };

        _mockStorage.Setup(s => s.GetUserByEmail(request.Email)).Returns(user);
        _mockHasher.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

        var (success, message, response) = _service.Login(request);

        Assert.False(success);
        Assert.Contains("Invalid email or password", message);
        Assert.Null(response);
    }

    [Fact]
    public void Login_WhenCredentialsCorrect_ShouldReturnJwtToken()
    {
        var request = new LoginRequestDto
        {
            Email = "user@techroad.dev",
            Password = "CorrectPassword123!"
        };

        var user = new User
        {
            Username = "UserOne",
            Email = request.Email,
            PasswordHash = "$2a$11$HashedPassword"
        };

        _mockStorage.Setup(s => s.GetUserByEmail(request.Email)).Returns(user);
        _mockHasher.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
        _mockHasher.Setup(h => h.IsHashed(user.PasswordHash)).Returns(true);

        var testExpires = DateTime.UtcNow.AddHours(24);
        _mockToken.Setup(t => t.GenerateToken(user, out testExpires)).Returns("valid.jwt.token");

        var (success, message, response) = _service.Login(request);

        Assert.True(success);
        Assert.NotNull(response);
        Assert.Equal("valid.jwt.token", response.Token);
        Assert.Equal(user.Username, response.Username);
    }
}
