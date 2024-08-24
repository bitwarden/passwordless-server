using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.AdminConsole.Components.Pages.Account;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Tests.DataFactory;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.Account;

public class MagicTests : TestContext
{
    public MagicTests()
    {
        Services.AddSingleton<MagicLinkSignInManager<ConsoleAdmin>>(new FakeMagicLinkSignInManager());
    }

    [InlineData("")]
    [Theory]
    public void OnInitializedAsync_WhenTokenIsEmpty_ShouldSetSuccessToFalse(string? token)
    {
        // Arrange
        var cut = RenderComponent<Magic>(p => p
            .AddCascadingValue("Token", token));

        // Act

        // Assert
        Assert.NotNull(cut.Find("#invalid-magic-link-token-alert"));
    }
}