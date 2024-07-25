using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class EmailAddressValidatorTests
{
    [InlineData("test@example.com")]
    [InlineData("test@subdomain.example.com")]
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

    [InlineData("a")]
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