using FCG02.UsersAPI.Application.DTOs;
using FCG02.UsersAPI.Application.Events;
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Application.Services;
using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.Enums;
using FCG02.UsersAPI.Domain.Interfaces;
using FCG02.UsersAPI.Domain.ValueObjects;
using Moq;

namespace FCG02.UsersAPI.UnitTests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _userService = new UserService(_userRepositoryMock.Object, _passwordHasherMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateUserAndPublishEvent()
    {
        // Arrange
        var request = new RegisterUserRequest("Gamer Pro", "gamer.pro@fiap.com.br", "SenhaForte123!", UsersAPI.Domain.Enums.UserRole.Gamer);
        var expectedHash = "$2a$11$hashCalculadoComSucesso...";

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(h => h.HashPassword(request.Password))
            .Returns(expectedHash);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _userService.RegisterAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(UserRole.Gamer, response.Role);

        _userRepositoryMock
            .Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);

        _eventPublisherMock
            .Verify(p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterUserRequest("Gamer Pro", "gamer.pro@fiap.com.br", "SenhaForte123!");

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterAsync(request));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Nome Teste", new Email("teste@fiap.com.br"), "hash", UserRole.Gamer);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email.Value, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }
}