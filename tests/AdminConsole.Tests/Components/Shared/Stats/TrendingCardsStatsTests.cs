using Bunit;
using Passwordless.AdminConsole.Components.Shared.Stats;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Stats;

public class TrendingCardsStatsTests : BunitContext
{
    [Fact]
    public void TrendingCardsStats_Renders_Nothing_WhenItemsAreEmpty()
    {
        // Arrange
        var cut = Render<TrendingCardsStats>(parameters => parameters
            .Add(p => p.Items, new List<TrendingCardsStats.Item>(0)));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void TrendingCardsStats_Renders_3Cards()
    {
        // Arrange
        var items = new List<TrendingCardsStats.Item>
        {
            new("Title 1", 1, TrendingCardsStats.ValueTypes.Integer, "Sub {0}", 0.04, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 2", 2, TrendingCardsStats.ValueTypes.Double, "Sub {0}", 0.02, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 3", 3, TrendingCardsStats.ValueTypes.Percentage, "Sub {0}", 1, TrendingCardsStats.ValueTypes.Integer),
        };
        var cut = Render<TrendingCardsStats>(parameters => parameters
            .Add(p => p.Items, items));

        // Act

        // Assert
        cut.MarkupMatches("<dl required diff:ignoreAttributes><div diff:ignore></div><div diff:ignore></div><div diff:ignore></div></dl>");
    }

    [Fact]
    public void TrendingCardsStats_Renders_ExpectedCardsWithExpectedContent()
    {
        // Arrange
        var items = new List<TrendingCardsStats.Item>
        {
            new("Title 1", 1, TrendingCardsStats.ValueTypes.Integer, "Sub {0}", 0.04, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 2", 2, TrendingCardsStats.ValueTypes.Double, "Sub {0}", 0.02, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 3", 3, TrendingCardsStats.ValueTypes.Percentage, "Sub {0}", 1, TrendingCardsStats.ValueTypes.Integer),
        };
        var cut = Render<TrendingCardsStats>(parameters => parameters
            .Add(p => p.Items, items));

        // Act

        // Assert
        var cards = cut.Nodes[0].ChildNodes;
        cards[0].MarkupMatches("<div diff:ignoreAttributes><div diff:ignoreAttributes><div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 1</dt><dd diff:ignoreAttributes>Sub 4.00%</dd><dd diff:ignoreAttributes>1</dd></div></div></div>");
        cards[1].MarkupMatches("<div diff:ignoreAttributes><div diff:ignoreAttributes><div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 2</dt><dd diff:ignoreAttributes>Sub 2.00%</dd><dd diff:ignoreAttributes>2.00</dd></div></div></div>");
        cards[2].MarkupMatches("<div diff:ignoreAttributes><div diff:ignoreAttributes><div diff:ignoreAttributes><dt diff:ignoreAttributes>Title 3</dt><dd diff:ignoreAttributes>Sub 1</dd><dd diff:ignoreAttributes>300.00%</dd></div></div></div>");
    }

    [Fact]
    public void TrendingCardsStats_Renders_ExpectedCardsWithExpectedClasses()
    {
        // Arrange
        var items = new List<TrendingCardsStats.Item>
        {
            new("Title 1", 1, TrendingCardsStats.ValueTypes.Integer, "Sub {0}", 0.04, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 2", 2, TrendingCardsStats.ValueTypes.Double, "Sub {0}", 0.02, TrendingCardsStats.ValueTypes.Percentage),
            new("Title 3", 3, TrendingCardsStats.ValueTypes.Percentage, "Sub {0}", 1, TrendingCardsStats.ValueTypes.Integer),
        };
        var cut = Render<TrendingCardsStats>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object> { { "class", "i-hate-css" } }));

        // Act

        // Assert
        cut.MarkupMatches("<dl diff:ignoreChildren class=\"mt-5 grid grid-cols-1 gap-5 sm:grid-cols-3 i-hate-css\"></dl>");
    }
}