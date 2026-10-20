using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FCG02.UsersAPI.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not found in configuration.");
        var issuer = _configuration["Jwt:Issuer"] ?? "FCG02.UsersAPI";
        var audience = _configuration["Jwt:Audience"] ?? "FCG02.CloudGamesClients";
        var expirationInSeconds = GetTokenExpirationInSeconds();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(expirationInSeconds),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }

    public int GetTokenExpirationInSeconds()
    {
        return int.TryParse(_configuration["Jwt:ExpirationInSeconds"], out var exp) ? exp : 7200;
    }
}