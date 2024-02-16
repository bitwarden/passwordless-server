using Bunit;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Stats;

public class SimpleCardsStatsTests : TestContext
{
    [Fact]
    public void SimpleCardsStats_Renders_Nothing_WhenItemsAreEmpty()
    {
        // Arrange
        var cut = RenderComponent<SimpleCardsStats>(parameters => parameters
            .Add(p => p.Items, new List<SimpleCardsStats.Item>(0)));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void SimpleCardsStats_Renders_3Cards()
    {
        // Arrange
        var items = new List<SimpleCardsStats.Item>
        {
            new("Title 1", 1, SimpleCardsStats.ValueTypes.Integer),
            new("Title 2", 2, SimpleCardsStats.ValueTypes.Double),
            new("Title 3", 3, SimpleCardsStats.ValueTypes.Percentage),
        };
        var cut = RenderComponent<SimpleCardsStats>(parameters => parameters
            .Add(p => p.Items, items));

        // Act

        // Assert
        cut.MarkupMatches("<dl required diff:ignoreAttributes><div diff:ignore></div><div diff:ignore></div><div diff:ignore></div></dl>");
    }

    [Fact]
    public void SimpleCardsStats_Renders_ExpectedCardsWithExpectedContent()
    {
        // Arrange
        var items = new List<SimpleCardsStats.Item>
        {
            new("Title 1", 1, SimpleCardsStats.ValueTypes.Integer),
            new("Title 2", 2, SimpleCardsStats.ValueTypes.Double),
            new("Title 3", 3, SimpleCardsStats.ValueTypes.Percentage),
        };
        var cut = RenderComponent<SimpleCardsStats>(parameters => parameters
            .Add(p => p.Items, items));

        // Act

        // Assert
        var cards = cut.Nodes[0].ChildNodes;
        cards[0].MarkupMatches("<div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 1</dt><dd diff:ignoreAttributes>1</dd></div>");
        cards[1].MarkupMatches("<div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 2</dt><dd diff:ignoreAttributes>2.00</dd></div>");
        cards[2].MarkupMatches("<div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 3</dt><dd diff:ignoreAttributes>300.00 %</dd></div>");
    }

    [Fact]
    public void SimpleCardsStats_Renders_ExpectedCardsWithExpectedClasses()
    {
        // Arrange
        var items = new List<SimpleCardsStats.Item>
        {
            new("Title 1", 1, SimpleCardsStats.ValueTypes.Integer),
            new("Title 2", 2, SimpleCardsStats.ValueTypes.Double),
            new("Title 3", 3, SimpleCardsStats.ValueTypes.Percentage),
        };
        var cut = RenderComponent<SimpleCardsStats>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "class", "i-hate-css" } }));

        // Act

        // Assert
        cut.MarkupMatches("<dl diff:ignoreChildren class=\"mt-5 grid grid-cols-1 gap-5 sm:grid-cols-3 i-hate-css\"></dl>");
    }
}