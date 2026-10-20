using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Application.Services;
using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.Enums;
using FCG02.UsersAPI.Domain.Interfaces;
using FCG02.UsersAPI.Domain.ValueObjects;
using Moq;

namespace FCG02.UsersAPI.UnitTests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var emailStr = "gamer@fiap.com.br";
        var password = "SenhaCorreta123!";
        var user = new User("Gamer", new Email(emailStr), "hashCorreto", UserRole.Gamer);
        var expectedToken = "jwt.token.valido";

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(emailStr, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.VerifyPassword(password, user.PasswordHash))
                           .Returns(true);

        _tokenServiceMock.Setup(t => t.GenerateToken(user))
                         .Returns(expectedToken);

        _tokenServiceMock.Setup(t => t.GetTokenExpirationInSeconds())
                         .Returns(7200);

        // Act
        var response = await _authService.LoginAsync(new LoginRequest(emailStr, password));

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedToken, response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(7200, response.ExpiresInSeconds);
        Assert.Equal(user.Name, response.User.Name);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest("inexistente@fiap.com.br", "qualquerSenha");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var emailStr = "gamer@fiap.com.br";
        var user = new User("Gamer", new Email(emailStr), "hashCorreto", UserRole.Gamer);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(emailStr, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.VerifyPassword("SenhaErrada", user.PasswordHash))
                           .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(new LoginRequest(emailStr, "SenhaErrada")));
    }
}