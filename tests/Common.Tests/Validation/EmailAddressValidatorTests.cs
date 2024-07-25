using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class EmailAddressValidatorTests
{
    [InlineData("example@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("x@example.com")]
    [InlineData("example-indeed@strange-example.com")]
    [InlineData("example@sub.example.com")]
    [Theory]
    public void IsValid_ReturnsTrue_WhenEmailIsValid(string email)
    {
        // Arrange

        // Act
        var actual = EmailAddressValidator.IsValid(email);

        // Assert
        Assert.True(actual);
    }

    [InlineData(null)]
    [InlineData("")]
    [Theory]
    public void IsValid_ReturnsFalse_WhenEmailIsNullOrEmpty(string? email)
    {
        // Arrange

        // Act
        var actual = EmailAddressValidator.IsValid(email);

        // Assert
        Assert.False(actual);
    }


    [InlineData("invalid-email")]
    [InlineData("example@")]
    [InlineData("@example.com")]
    [InlineData("example@.com")]
    [InlineData("example@com")]
    [InlineData("example@domain..com")]
    [Theory]
    public void IsValid_ReturnsFalse_WhenEmailIsInvalid(string email)
    {
        // Arrange

        // Act
        var actual = EmailAddressValidator.IsValid(email);

        // Assert
        Assert.False(actual);
    }
}