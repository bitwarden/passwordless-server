using Bunit;
using Passwordless.AdminConsole.Components.Shared;
using Passwordless.AdminConsole.Components.Shared.Alerts;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Alerts;

public class LinkAlertTests : BunitContext
{
    [InlineData(ContextualStyles.Danger, "container-danger")]
    [InlineData(ContextualStyles.Info, "container-info")]
    [InlineData(ContextualStyles.Success, "container-success")]
    [InlineData(ContextualStyles.Warning, "container-warning")]
    [InlineData(ContextualStyles.Primary, "container-primary")]
    [InlineData(ContextualStyles.Secondary, "container-secondary")]
    [Theory]
    public void Renders_ExpectedContainerClass_WhenContextualStyleIsDanger(ContextualStyles style, string expectedClass)
    {
        // Arrange
        var cut = Render<LinkAlert>(parameters => parameters
            .Add(p => p.LinkUrl, "https://www.example.com")
            .Add(p => p.Message, "Test Message")
            .Add(p => p.LinkText, "Test Link Text")
            .Add(p => p.Style, style));

        // Act

        // Assert
        cut.MarkupMatches($"<div class=\"{expectedClass}\" diff:ignoreChildren></div>");
    }

    [InlineData(ContextualStyles.Danger, "content-danger")]
    [InlineData(ContextualStyles.Info, "content-info")]
    [InlineData(ContextualStyles.Success, "content-success")]
    [InlineData(ContextualStyles.Warning, "content-warning")]
    [InlineData(ContextualStyles.Primary, "content-primary")]
    [InlineData(ContextualStyles.Secondary, "content-secondary")]
    [Theory]
    public void Renders_ExpectedContentClass_WhenContextualStyleIsDanger(ContextualStyles style, string expectedClass)
    {
        // Arrange
        var cut = Render<LinkAlert>(parameters => parameters
            .Add(p => p.LinkUrl, "https://www.example.com")
            .Add(p => p.Message, "Test Message")
            .Add(p => p.LinkText, "Test Link Text")
            .Add(p => p.Style, style));

        // Act

        // Assert
        cut.MarkupMatches($"<div diff:ignore><div diff:ignore><div diff:ignore></div><div class=\"{expectedClass}\" diff:ignoreChildren></div></div></div>");
    }
}