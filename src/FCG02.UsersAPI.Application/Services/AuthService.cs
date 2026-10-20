using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Domain.Interfaces;
using FCG02.UsersAPI.Domain.ValueObjects;

namespace FCG02.UsersAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        // Validate email format
        var email = new Email(request.Email);

        // seek user by email
        var user = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Generate token
        var token = _tokenService.GenerateToken(user);
        var expiration = _tokenService.GetTokenExpirationInSeconds();

        // Return response
        return new AuthResponse
        (
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresInSeconds: expiration,
            User: UserResponse.FromEntity(user)
        );

    }
}