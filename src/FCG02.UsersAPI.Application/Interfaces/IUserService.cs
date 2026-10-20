using FCG02.UsersAPI.Application.DTOs;

namespace FCG02.UsersAPI.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}