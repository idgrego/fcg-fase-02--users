using FCG02.UsersAPI.Application.DTOs;

namespace FCG02.UsersAPI.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}