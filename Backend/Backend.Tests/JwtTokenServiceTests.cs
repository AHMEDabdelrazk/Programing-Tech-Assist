using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using TechRoad.API.Models;
using TechRoad.API.Services;
using Xunit;

namespace TechRoad.Tests;

public class JwtTokenServiceTests
{
    private readonly IConfiguration _config;
    private readonly JwtTokenService _tokenService;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "VeryLongSecretKeyForTestingPurposesOnly1234567890!"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiresInMinutes", "60"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new JwtTokenService(_config);
    }

    [Fact]
    public void GenerateToken_ShouldProduceValidJwtWithExpectedClaims()
    {
        var user = new User
        {
            Id = "user-123",
            Username = "TestDeveloper",
            Email = "developer@techroad.dev"
        };

        var tokenString = _tokenService.GenerateToken(user, out var expiresAt);

        Assert.NotNull(tokenString);
        Assert.True(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);

        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value;
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

        Assert.Equal(user.Username, usernameClaim);
        Assert.Equal(user.Email, emailClaim);
    }
}
