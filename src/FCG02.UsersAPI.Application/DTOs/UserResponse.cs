using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.Enums;

namespace FCG02.UsersAPI.Application.DTOs;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    DateTime CreatedAt
)
{
    public static UserResponse FromEntity(User user) =>
        new UserResponse(
            Id: user.Id,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role,
            CreatedAt: user.CreatedAt
        );
}