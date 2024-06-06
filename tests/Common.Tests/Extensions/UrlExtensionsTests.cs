using Passwordless.Common.Extensions;

namespace Passwordless.Common.Tests.Extensions;

public class UrlExtensionsTests
{
    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsEmpty_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = string.Empty;

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsSlash_ThenShouldReturnTrue()
    {
        // Arrange
        var sut = "/";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsSlashFoo_ThenShouldReturnTrue()
    {
        // Arrange
        var sut = "/foo";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsDoubleSlash_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = "//";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsSlashBackslash_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = "/\\";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsTildeSlash_ThenShouldReturnTrue()
    {
        // Arrange
        var sut = "~/";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsTildeSlashFoo_ThenShouldReturnTrue()
    {
        // Arrange
        var sut = "~/foo";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsTildeDoubleSlash_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = "~//";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlIsTildeBackslash_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = "~/\\";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsLocalUrl_GivenUrl_WhenUrlContainsControlCharacter_ThenShouldReturnFalse()
    {
        // Arrange
        var sut = "/\u0001";

        // Act
        var actual = sut.IsLocalUrl();

        // Assert
        Assert.False(actual);
    }
}