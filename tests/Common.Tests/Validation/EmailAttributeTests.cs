using System.ComponentModel.DataAnnotations;
using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class EmailAttributeTests
{
    [Theory]
    [MemberData(nameof(EmailData.Valid), MemberType = typeof(EmailData))]
    public void ValidationAttribute_ReturnsIsValid_WhenEmailIsValid(string email)
    {
        // Arrange
        var target = new EmailValidationTarget { Email = email };

        // Act
        var actual = AreAttributesValid(target);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.Valid), MemberType = typeof(EmailData))]
    [MemberData(nameof(EmailData.ValidInternational), MemberType = typeof(EmailData))]
    public void ValidationAttribute_ReturnsIsValid_WhenInternationalEmailIsValid(string email)
    {
        // Arrange
        var target = new InternationalEmailValidationTarget { Email = email };

        // Act
        var actual = AreAttributesValid(target);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(EmailData.Invalid), MemberType = typeof(EmailData))]
    [MemberData(nameof(EmailData.InvalidInternational), MemberType = typeof(EmailData))]
#pragma warning disable xUnit1026
    public void ValidationAttribute_ReturnsIsInvalid(string email, EmailValidationErrorCode errorCode, int x, int y)
    {
        // Arrange
        var target = new EmailValidationTarget { Email = email };

        // Act
        var actual = AreAttributesValid(target);

        // Assert
        Assert.False(actual);
    }
#pragma warning restore xUnit1026


    [Fact]
    public void EmailAttribute_IsValid_WhenEmailIsNull()
    {
        // Arrange
        var target = new EmailValidationTarget { Email = null! };

        // Act
        var actual = AreAttributesValid(target);

        // Assert

        Assert.True(actual);
    }

    static bool AreAttributesValid(object target)
    {
        var context = new ValidationContext(target, null, null);
        var results = new List<ValidationResult>();

        return Validator.TryValidateObject(target, context, results, true);
    }

    class EmailValidationTarget
    {
        [Email(true)] public string Email { get; set; }
    }

    class InternationalEmailValidationTarget
    {
        [Email(true, true)] public string Email { get; set; }
    }
}