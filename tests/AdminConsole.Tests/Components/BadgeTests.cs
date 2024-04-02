using Bunit;
using Passwordless.AdminConsole.Components.Shared;
using Passwordless.AdminConsole.TagHelpers;
using Xunit;
using Badge = Passwordless.AdminConsole.Components.Shared.Badge;

namespace Passwordless.AdminConsole.Tests.Components;

public class BadgeTests : TestContext
{
    #region Class
    [Theory]
    [InlineData(ContextualStyles.Primary)]
    [InlineData(ContextualStyles.Success)]
    [InlineData(ContextualStyles.Danger)]
    [InlineData(ContextualStyles.Warning)]
    [InlineData(ContextualStyles.Info)]
    public void Badge_Renders_WhiteText(ContextualStyles variant)
    {
        // Arrange
        var cut = RenderComponent<Badge>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Contains("text-white", actual);
    }

    public static IEnumerable<object[]> BackgroundClassData = new List<object[]>
    {
        new object[] {ContextualStyles.Primary, "bg-blue-600"},
        new object[] {ContextualStyles.Success, "bg-green-600"},
        new object[] {ContextualStyles.Danger, "bg-red-600"},
        new object[] {ContextualStyles.Warning, "bg-yellow-600"},
        new object[] {ContextualStyles.Info, "bg-blue-600"}
    };

    [Theory]
    [MemberData(nameof(BackgroundClassData))]
    public void Badge_Renders_ExpectedBackground_Variant(ContextualStyles variant, string expectedClass)
    {
        // Arrange
        var cut = RenderComponent<Badge>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Contains(expectedClass, actual);
    }
    #endregion

    #region Message
    [Fact]
    public void Badge_Renders_ExpectedText()
    {
        // Arrange
        var cut = RenderComponent<Badge>(parameters => parameters
            .Add(p => p.Message, "Active"));
        var span = cut.Find("span");

        // Act

        // Assert
        Assert.Contains("Active", span.TextContent);
        Assert.Contains("Active", span.InnerHtml);
    }
    #endregion
}