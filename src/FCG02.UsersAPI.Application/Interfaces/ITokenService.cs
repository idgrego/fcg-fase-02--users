using FCG02.UsersAPI.Domain.Entities;

namespace FCG02.UsersAPI.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    int GetTokenExpirationInSeconds();
}