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
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("<p>hello</p>")]
    [InlineData("<a href='https://example.com'>hello</a>")]
    [InlineData("<strong>hello</strong>")]
    public void Sanitize_WhenGivenHtmlWithAllowedTags_ShouldReturnSanitizedHtml(string input)
    {
        // Arrange

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        result.Should().BeEmpty();
    }
}