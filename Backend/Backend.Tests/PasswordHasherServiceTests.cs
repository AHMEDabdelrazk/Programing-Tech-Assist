using TechRoad.API.Services;
using Xunit;

namespace TechRoad.Tests;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnValidBcryptHash()
    {
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.True(_hasher.IsHashed(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "MySecretPassword456";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "MySecretPassword456";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword("WrongPassword", hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithLegacyPlaintext_ShouldMatchDirectly()
    {
        var plainPassword = "LegacyPlainPassword";

        var result = _hasher.VerifyPassword(plainPassword, plainPassword);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void VerifyPassword_WithEmptyInput_ShouldReturnFalse(string? input)
    {
        Assert.False(_hasher.VerifyPassword(input!, "someHash"));
        Assert.False(_hasher.VerifyPassword("pass", input!));
    }
}
