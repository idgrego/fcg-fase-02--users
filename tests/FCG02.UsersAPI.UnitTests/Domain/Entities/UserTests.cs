using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.Enums;
using FCG02.UsersAPI.Domain.ValueObjects;

namespace FCG02.UsersAPI.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Email _validEmail = new Email("gamer@fiap.com.br");
    private const string ValidName = "Gamer Teste";
    private const string ValidPasswordHash = "abc498asb3";

    [Fact]
    public void Constructor_WithValidParameters_ShouldInstantiateUserSuccessfully()
    {
        // Arrange & Act
        var user = new User(ValidName, _validEmail, ValidPasswordHash, UserRole.Gamer);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(ValidName, user.Name);
        Assert.Equal(_validEmail, user.Email);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.Equal(UserRole.Gamer, user.Role);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User(invalidName!, _validEmail, ValidPasswordHash, UserRole.Gamer));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        //Act & Assert
        Assert.Throws<ArgumentNullException>(() => new User(ValidName, null!, ValidPasswordHash, UserRole.Gamer));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidPasswordHash_ShouldThrowArgumentException(string? invalidPasswordHash)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User(ValidName, _validEmail, invalidPasswordHash!, UserRole.Gamer));
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndSetUpdatedAt()
    {
        // Arrange
        var user = new User(ValidName, _validEmail, ValidPasswordHash, UserRole.Gamer);
        var newName = "Updated Gamer";

        // Act
        user.UpdateName(newName);

        // Assert
        Assert.Equal(newName, user.Name);
        Assert.NotNull(user.UpdatedAt);
        Assert.True(user.UpdatedAt > user.CreatedAt);
    }

    [Fact]
    public void UpdateEmail_WithValidNewMail_ShouldUpdateEmailAndSetUpdatedAt()
    {
        // Arrange
        var user = new User(ValidName, _validEmail, ValidPasswordHash, UserRole.Gamer);
        var newEmail = new Email("newemail@fiap.com.br");

        // Act 
        user.UpdateEmail(newEmail);

        // Assert 
        Assert.Equal(newEmail, user.Email);
        Assert.NotNull(user.UpdatedAt);
        Assert.True(user.UpdatedAt > user.CreatedAt);
    }

    [Fact]
    public void UpdateRole_WithValidNewRole_ShouldUpdateRoleAndSetUpdatedAt()
    {
        // Arrange
        var user = new User(ValidName, _validEmail, ValidPasswordHash, UserRole.Gamer);
        var newRole = UserRole.Admin;

        // Act
        user.UpdateRole(newRole);

        // Assert
        Assert.Equal(newRole, user.Role);
        Assert.NotNull(user.UpdatedAt);
        Assert.True(user.UpdatedAt > user.CreatedAt);
    }
}