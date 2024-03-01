using Passwordless.Common.Validation;

namespace Passwordless.Common.Tests.Validation;

public class NoForbiddenContentAttributeTests
{
    private readonly NoForbiddenContentAttribute _sut = new();
    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsTrue()
    {
        // Arrange
        var value = (string?)null;

        // Act
        var actual = _sut.IsValid(value);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_WhenValueIsNotString_ThrowsArgumentException()
    {
        // Arrange
        var value = new object();

        // Act
        var actual = Assert.Throws<ArgumentException>(() => _sut.IsValid(value));

        // Assert
        Assert.Equal("This attribute can only be applied to strings.", actual.Message);
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<div>alert</div>")]
    [InlineData("<a>alert</a>")]
    public void IsValid_WhenValueContainsHtmlTags_ReturnsFalse(string value)
    {
        // Act
        var actual = _sut.IsValid(value);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_WhenValueDoesNotContainHtmlTags_ReturnsTrue()
    {
        // Arrange
        var value = "This is a string without HTML tags";

        // Act
        var actual = _sut.IsValid(value);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData("http://")]
    [InlineData("https://")]
    public void IsValid_WhenValueContainsSchemes_ReturnsFalse(string value)
    {
        // Act
        var actual = _sut.IsValid(value);

        // Assert
        Assert.False(actual);
    }
}