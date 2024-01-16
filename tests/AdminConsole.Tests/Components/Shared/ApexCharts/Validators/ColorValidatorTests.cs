using Passwordless.AdminConsole.Components.Shared.ApexCharts.Validators;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.ApexCharts.Validators;

public class ColorValidatorTests
{

    [Fact]
    public void IsValid_ReturnsTrue_WhenRgbHexIsValid()
    {
        // Arrange
        var hex = "#000000";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenRgbaHexIsValid()
    {
        // Arrange
        var hex = "#000000FF";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenUppercaseHexIsValid()
    {
        // Arrange
        var hex = "#ABCDEFAB";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenLowercaseHexIsValid()
    {
        // Arrange
        var hex = "#abcdefab";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenLengthIsTooLong()
    {
        // Arrange
        var hex = "#abcdefaba";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenLengthIsTooShort1()
    {
        // Arrange
        var hex = "#abcde";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenLengthIsTooShort2()
    {
        // Arrange
        var hex = "#ab";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenLengthIs3()
    {
        // Arrange
        var hex = "#abc";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenLengthIs4()
    {
        // Arrange
        var hex = "#abcd";

        // Act
        var actual = ColorValidator.IsValid(hex);

        // Assert
        Assert.True(actual);
    }
}