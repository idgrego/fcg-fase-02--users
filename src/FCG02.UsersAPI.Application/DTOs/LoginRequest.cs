namespace FCG02.UsersAPI.Application.DTOs;

public record LoginRequest(
    string Email,
    string Password
);