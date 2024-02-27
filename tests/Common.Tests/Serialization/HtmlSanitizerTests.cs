using System.Runtime.InteropServices;
using FluentAssertions;
using Passwordless.Common.Serialization;

namespace Passwordless.Common.Tests.Serialization;

public class HtmlSanitizerTests
{
    [Fact]
    public void Sanitize_WhenGivenHtml_ShouldReturnSanitizedHtml()
    {
        // Arrange
        var input = "<script>alert('hello');</script>";

        // Act
        var actual = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Equal("alert('hello');", actual);
    }

    [Theory]
    [InlineData("<p>hello</p>")]
    [InlineData("<a href='https://example.com'>hello</a>")]
    [InlineData("<strong>hello</strong>")]
    [InlineData("<a><a>hello</a></a>")]
    public void Sanitize_WhenGivenHtmlWithAllowedTags_ShouldReturnSanitizedHtml(string input)
    {
        // Arrange

        // Act
        var actual = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Equal("hello", actual);
    }
}