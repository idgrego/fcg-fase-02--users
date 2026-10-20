using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Domain.Enums;

namespace FCG02.UsersAPI.Application.Events;

public record UserCreatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid UserId,
    string Name,
    string Email,
    UserRole Role
);