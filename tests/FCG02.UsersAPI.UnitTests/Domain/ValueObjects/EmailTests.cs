using FCG02.UsersAPI.Domain.ValueObjects;

namespace FCG02.UsersAPI.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("gamer@fiap.com.br")]
    [InlineData("admin.user@cloudgames.io")]
    [InlineData("test+tag@sub.domain.org")]
    public void Constructor_WithValidEmail_ShouldCreateInstaceAndNormalize(string input)
    {
        // act
        var email = new Email(input);

        // assert
        Assert.Equal(input.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithNullOrEmptyEmail_ShouldThrowArgumentException(string? invalidInput)
    {
        // act & assert
        Assert.Throws<ArgumentException>(() => new Email(invalidInput!));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string invalidInput)
    {
        // act & assert
        Assert.Throws<ArgumentException>(() => new Email(invalidInput));
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // arrange
        var email1 = new Email("user@domain.com");
        var email2 = new Email("user@domain.com");

        // act
        var result = email1.Equals(email2);

        // assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithSameValuesDifferentCasingOrSpacing_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("contato@fiap.com.br");
        var email2 = new Email("CONTATO@fiap.com.br ");

        // Assert (Igualdade estrutural de record)
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var email = new Email("jogador@fiap.com.br");

        // Act
        string emailString = email;

        // Assert
        Assert.Equal("jogador@fiap.com.br", emailString);
    }
}