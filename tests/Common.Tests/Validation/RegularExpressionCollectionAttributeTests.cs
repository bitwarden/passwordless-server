using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class RegularExpressionCollectionAttributeTests
{
    [Fact]
    public void IsValid_Returns_True_WhenInputIsNull()
    {
        // Arrange
        var sut = new RegularExpressionCollectionAttribute("^[a-z]+$");

        // Act
        var actual = sut.IsValid(null);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_Returns_True_WhenRegularExpressionMatches()
    {
        // Arrange
        var input = new[] { "hello", "world" };
        var sut = new RegularExpressionCollectionAttribute("^[a-z]+$");

        // Act
        var actual = sut.IsValid(input);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_Returns_False_WhenRegularExpressionDoesNotMatch()
    {
        // Arrange
        var input = new[] { "Hello", "World" };
        var sut = new RegularExpressionCollectionAttribute("^[a-z]+$");

        // Act
        var actual = sut.IsValid(input);

        // Assert
        Assert.False(actual);
    }
}