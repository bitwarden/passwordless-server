using AutoFixture;
using Bunit;
using Passwordless.AdminConsole.Components.Shared.Modals;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Modals;

public class SimpleAlertTests : TestContext
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void SimpleAlert_Applies_HiddenClass_WhenIsHiddenIsTrue()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, true)
            .Create();

        // Act
        var cut = RenderComponent<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find("div[aria-modal=\"true\"]");
        Assert.NotNull(actual);
        Assert.Contains("hidden", actual.ClassList);
    }

    [Fact]
    public void SimpleAlert_DoesNotApply_HiddenClass_WhenIsHiddenIsFalse()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .Create();

        // Act
        var cut = RenderComponent<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find("div[aria-modal=\"true\"]");
        Assert.NotNull(actual);
        Assert.DoesNotContain("hidden", actual.ClassList);
    }

    [Fact]
    public void SimpleAlert_Renders_IdAttribute()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .Create();

        // Act
        var cut = RenderComponent<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find("div[aria-modal=\"true\"]");
        Assert.NotNull(actual);
        Assert.Equal(model.Id, actual.Id);
    }

    [Fact]
    public void SimpleAlert_Renders_Title()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .Create();

        // Act
        var cut = RenderComponent<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find("h3");
        Assert.NotNull(actual);
        Assert.Equal(model.Title, actual.TextContent);
    }
}