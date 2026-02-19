using System.Collections.Immutable;
using AutoFixture;
using Bunit;
using Passwordless.AdminConsole.Components.Shared.Modals;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Modals;

public class SimpleAlertTests : BunitContext
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
        var cut = Render<SimpleAlert>(parameters => parameters
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
        var cut = Render<SimpleAlert>(parameters => parameters
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
        var cut = Render<SimpleAlert>(parameters => parameters
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
        var cut = Render<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find($"#{model.Id}-title");
        Assert.NotNull(actual);
        Assert.Equal(model.Title, actual.TextContent);
    }

    [Fact]
    public void SimpleAlert_Renders_Description()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .Create();

        // Act
        var cut = Render<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find($"#{model.Id}-description");
        Assert.NotNull(actual);
        Assert.Equal(model.Description, actual.TextContent);
    }

    [Fact]
    public void SimpleAlert_Renders_Actions_WithDefaultText()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .Without(x => x.ConfirmText)
            .Without(x => x.CancelText)
            .Create();

        // Act
        var cut = Render<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find($"#{model.Id}-actions");
        var buttons = actual.Children.Where(x => x.NodeName == "BUTTON").ToImmutableList();
        Assert.Equal(2, buttons.Count);
        Assert.Equal("Confirm", buttons[0].TextContent);
        Assert.Equal("Cancel", buttons[1].TextContent);
    }

    /// <summary>
    /// If this test requires to be fixed, it would mean any Javascript-based event handlers would be broken.
    /// </summary>
    [Fact]
    public void SimpleAlert_Renders_Actions_WithExpectedIdAttributes()
    {
        // Arrange
        var model = _fixture.Build<SimpleAlert.SimpleAlertModel>()
            .With(x => x.IsHidden, false)
            .With(x => x.Id, "my-dialog")
            .Create();

        // Act
        var cut = Render<SimpleAlert>(parameters => parameters
            .Add(p => p.Model, model));

        // Assert
        var actual = cut.Find($"#{model.Id}-actions");
        var buttons = actual.Children.Where(x => x.NodeName == "BUTTON").ToImmutableList();
        Assert.Equal("my-dialog-action-confirm", buttons[0].Id);
        Assert.Equal("my-dialog-action-cancel", buttons[1].Id);
    }
}