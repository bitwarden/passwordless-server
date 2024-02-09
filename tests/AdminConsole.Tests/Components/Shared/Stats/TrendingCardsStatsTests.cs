using Bunit;
using Passwordless.AdminConsole.Components.Shared.Stats;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Stats;

public class TrendingCardsStatsTests : TestContext
{
    [Fact]
    public void TrendingCardsStats_Renders_Nothing_WhenItemsAreEmpty()
    {
        // Arrange
        var cut = RenderComponent<TrendingCardsStats>(parameters => parameters
            .Add(p => p.Items, new List<TrendingCardsStats.Item>(0)));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Empty(actual);
    }
}