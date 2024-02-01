using AutoFixture;
using Bunit;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public sealed class BreadCrumbTests : TestContext
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void BreadCrumb_Renders_Nothing_WhenOnlyOneItem()
    {
        // Arrange

        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(1).ToList();
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void BreadCrumb_Renders_Nothing_WhenNoItems()
    {
        // Arrange
        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(0).ToList();
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void BreadCrumb_Renders_Nothing_WhenItemsIsNull()
    {
        // Arrange
        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(0).ToList();
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void BreadCrumb_Renders_LastItemAsText()
    {
        // Arrange
        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(2).ToList();

        // Act
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var orderedList = cut.Find("ol");
        var currentPageListItem = orderedList.Children.Last();
        Assert.Contains(currentPageListItem.Attributes, x => x is { Name: "aria-current", Value: "page" });
        Assert.DoesNotContain(currentPageListItem.Children.First().Children, x => x.NodeName == "A");
        Assert.Contains(currentPageListItem.Children.First().Children, x => x.NodeName == "SPAN");
        Assert.Equal(items.Last().Title, currentPageListItem.Children.First().Children.First(x => x.NodeName == "SPAN").TextContent);
    }

    [Fact]
    public void BreadCrumb_Renders_FirstElementAsLink()
    {
        // Arrange
        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(3).ToList();

        // Act
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var orderedList = cut.Find("ol");
        var firstListItem = orderedList.Children.First();
        Assert.Contains(firstListItem.Children, x => x.NodeName == "A");
        var firstLinkTag = firstListItem.Children.First(x => x.NodeName == "A");
        Assert.Contains(items.First().Title, firstLinkTag.TextContent);
        Assert.Equal(items.First().Url, firstLinkTag.Attributes["href"]!.Value);
    }

    [Fact]
    public void BreadCrumb_Renders_AllMiddleItemsButAsLinks()
    {
        // Arrange
        var items = _fixture.CreateMany<BreadCrumb.BreadCrumbItem>(3).ToList();

        // Act
        var cut = RenderComponent<BreadCrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var orderedList = cut.Find("ol");

        for (var i = 1; i < (items.Count - 1); i++)
        {
            var listItem = orderedList.Children[i];
            Assert.Contains(listItem.Children.First().Children, x => x.NodeName == "A");
            var linkTag = listItem.Children.First().Children.First(x => x.NodeName == "A");
            Assert.Contains(items[i].Title, linkTag.TextContent);
            Assert.Equal(items[i].Url, linkTag.Attributes["href"]!.Value);
        }
    }
}