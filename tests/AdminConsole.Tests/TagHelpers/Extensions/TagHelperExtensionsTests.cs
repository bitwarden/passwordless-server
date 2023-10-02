using AdminConsole.TagHelpers.Icons;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Passwordless.AdminConsole.TagHelpers.Extensions;
using Xunit;

namespace Passwordless.AdminConsole.Tests.TagHelpers.Extensions;

public class TagHelperExtensionsTests
{
    [Fact]
    public void RenderHtml_Renders_TagHelper_AsExpected()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new DangerAlertIcon();

        // act
        var actual = icon.RenderHtml(context.Object);

        // assert
        var expected = "<svg class=\"h-5 w-5 fill-red-400\" fill=\"currentColor\" viewBox=\"0 0 20 20\" aria-hidden=\"true\"><path fill-rule=\"evenodd\" d=\"M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z\" clip-rule=\"evenodd\"></path></svg>";
        Assert.Equal(expected, actual);
    }
}