using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class EmailValidationTests
{

    [Theory]
    [MemberData(nameof(EmailData.Valid), MemberType = typeof(EmailData))]
    public void Validate_ReturnsTrue_WhenValid(string email)
    {
        // Arrange

        // Act
        var actual = EmailValidator.Validate(email, true, false);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.Valid), MemberType = typeof(EmailData))]
    [MemberData(nameof(EmailData.ValidInternational), MemberType = typeof(EmailData))]
    public void Validate_ReturnsTrue_WhenInternationalValid(string email)
    {
        // Arrange

        // Act
        var actual = EmailValidator.Validate(email, true, true);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.Valid), MemberType = typeof(EmailData))]
    public void TryValidate_ReturnsTrue_WhenValid(string email)
    {
        // Arrange

        // Act
        var actual = EmailValidator.TryValidate(email, true, false, out _);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.ValidInternational), MemberType = typeof(EmailData))]
    public void TryValidate_ReturnsTrue_WhenInternationalValid(string email)
    {
        // Arrange

        // Act
        var actual = EmailValidator.TryValidate(email, true, true, out _);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.Invalid), MemberType = typeof(EmailData))]
    [MemberData(nameof(EmailData.InvalidInternational), MemberType = typeof(EmailData))]
    public void Validate_ReturnsFalse_WhenInvalid(string email, int y)
    {
        // Arrange

        // Act
        var actual = EmailValidator.Validate(email, true, false);


        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void TestArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EmailValidator.Validate(null, true, true));
        Assert.Throws<ArgumentNullException>(() => EmailValidator.TryValidate(null, true, true, out _));
    }
}