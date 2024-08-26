using Bunit;
using Microsoft.AspNetCore.Components;
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

    [Fact]
    public void OnInitializedAsync_RendersError_WhenTokenIsNotSpecified()
    {
        // Arrange
        var cut = RenderComponent<Magic>();

        // Act

        // Assert
        Assert.NotNull(cut.Find("#invalid-magic-link-token-alert"));
    }

    [Fact]
    public void OnInitializedAsync_DoesNotRender_WhenTokenIsSpecified()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var token = FakeMagicLinkSignInManager.SuccessToken;
        var uri = navigationManager.GetUriWithQueryParameter("token", token);
        navigationManager.NavigateTo(uri);

        var cut = this.RenderComponent<Magic>();

        // Act

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#invalid-magic-link-token-alert"));
    }

    [Fact]
    public void OnInitializedAsync_RendersError_WhenLoginFails()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var token = FakeMagicLinkSignInManager.FailToken;
        var uri = navigationManager.GetUriWithQueryParameter("token", token);
        navigationManager.NavigateTo(uri);

        var cut = this.RenderComponent<Magic>();

        // Act

        // Assert
        Assert.NotNull(cut.Find("#invalid-magic-link-token-alert"));
    }

    [Fact]
    public void OnInitializedAsync_NavigatesToOrganizationOverview_WhenReturnUrlIsNotSetDuringSuccessfulLogin()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var token = FakeMagicLinkSignInManager.SuccessToken;
        var uri = navigationManager.GetUriWithQueryParameter("token", token);
        navigationManager.NavigateTo(uri);

        RenderComponent<Magic>();

        // Act

        // Assert
        Assert.Equal("http://localhost/Organization/Overview", navigationManager.Uri);
    }

    [Fact]
    public void OnInitializedAsync_NavigatesToExpectedReturnUrl_WhenReturnUrlIsSetDuringSuccessfulLogin()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var token = FakeMagicLinkSignInManager.SuccessToken;
        var parameters = new Dictionary<string, object?>();
        parameters.Add("token", token);
        parameters.Add("returnUrl", "/Organization/Settings");
        var uri = navigationManager.GetUriWithQueryParameters(parameters);
        navigationManager.NavigateTo(uri);

        RenderComponent<Magic>();

        // Act

        // Assert
        Assert.Equal("http://localhost/Organization/Settings", navigationManager.Uri);
    }
}