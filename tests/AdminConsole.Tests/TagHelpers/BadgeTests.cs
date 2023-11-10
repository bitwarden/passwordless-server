using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Passwordless.AdminConsole.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Extensions;
using Xunit;

namespace Passwordless.AdminConsole.Tests.TagHelpers;

public class BadgeTests
{
    [Fact]
    public void RenderHtml_Renders_AlertBadge_AsExpected()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new Badge
        {
            Text = "Inactive",
            Variant = ColorVariant.Danger,
            Class = "invisible"
        };

        // act
        var actual = icon.RenderHtml(context.Object);

        // assert
        var expected = "<span class=\"rounded-md px-2 py-1 text-xs font-semibold bg-red-600 text-white invisible\">Inactive</span>";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RenderHtml_Renders_PrimaryBadge_AsExpected()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new Badge
        {
            Text = "Active",
            Variant = ColorVariant.Primary,
            Class = "invisible"
        };

        // act
        var actual = icon.RenderHtml(context.Object);

        // assert
        var expected = "<span class=\"rounded-md px-2 py-1 text-xs font-semibold bg-blue-600 text-white invisible\">Active</span>";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RenderHtml_Renders_PrimaryBadge_WhenClassIsNull()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new Badge
        {
            Text = "Active",
            Variant = ColorVariant.Primary,
            Class = null
        };

        // act
        var actual = icon.RenderHtml(context.Object);

        // assert
        var expected = "<span class=\"rounded-md px-2 py-1 text-xs font-semibold bg-blue-600 text-white\">Active</span>";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RenderHtml_Renders_PrimaryBadge_ByDefaultWhenVariantIsNotSpecified()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new Badge
        {
            Text = "Active",
        };

        // act
        var actual = icon.RenderHtml(context.Object);

        // assert
        var expected = "<span class=\"rounded-md px-2 py-1 text-xs font-semibold bg-blue-600 text-white\">Active</span>";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RenderHtml_Throws_ArgumentNullException_WhenTextIsNull()
    {
        // arrange
        var context = new Mock<TagHelperContext>(new TagHelperAttributeList(), new Dictionary<object, object>(), "unique-id");

        var icon = new Badge();

        // act
        Assert.Throws<ArgumentNullException>(() => icon.RenderHtml(context.Object));

        // assert
    }
}