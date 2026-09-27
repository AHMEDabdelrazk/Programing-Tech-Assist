using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TechRoad.API.Models;

namespace TechRoad.API.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, out DateTime expiresAt)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey must be configured through user secrets or an environment variable.");
        var issuer = _configuration["Jwt:Issuer"] ?? "ProgrammingTechAssistAPI";
        var audience = _configuration["Jwt:Audience"] ?? "ProgrammingTechAssistClient";
        var expirationMinutesStr = _configuration["Jwt:ExpiresInMinutes"] ?? "1440";

        if (!int.TryParse(expirationMinutesStr, out var expirationMinutes))
        {
            expirationMinutes = 1440;
        }

        expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
