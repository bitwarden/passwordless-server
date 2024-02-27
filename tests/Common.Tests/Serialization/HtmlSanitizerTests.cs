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
}