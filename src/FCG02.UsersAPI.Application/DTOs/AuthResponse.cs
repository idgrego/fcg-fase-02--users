namespace FCG02.UsersAPI.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    UserResponse User
);