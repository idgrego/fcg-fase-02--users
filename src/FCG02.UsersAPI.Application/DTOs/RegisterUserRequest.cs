using FCG02.UsersAPI.Domain.Enums;

namespace FCG02.UsersAPI.Application.DTOs;

public record RegisterUserRequest(
    string Name,
    string Email,
    string Password,
    UserRole Role = UserRole.Gamer
);
