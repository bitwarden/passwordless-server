using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Passwordless.AdminConsole.Components.Pages.Account;
using Passwordless.AdminConsole.Endpoints;
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
    public async Task Redirects_to_error_page_when_token_is_missing()
    {
        string token = null;
        
        var actual = await ComplimentaryEndpoints.AccountMagicEndpoint(token, null, new FakeMagicLinkSignInManager());
        
        var redirectResult = Assert.IsType<RedirectHttpResult>(actual);
        Assert.Equal("/Account/BadMagic", redirectResult.Url);
    }

    [Fact]
    public async Task Redirects_to_correct_page_when_token_is_valid()
    {
        var token = FakeMagicLinkSignInManager.SuccessToken;
        
        var actual = await ComplimentaryEndpoints.AccountMagicEndpoint(token, null, new FakeMagicLinkSignInManager());
        
        var redirectResult = Assert.IsType<RedirectHttpResult>(actual);
        Assert.Equal("/Organization/Overview", redirectResult.Url);
    }

    [Fact]
    public async Task Redirects_to_error_page_when_token_is_not_valid()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var token = FakeMagicLinkSignInManager.FailToken;
        
        
        var actual = await ComplimentaryEndpoints.AccountMagicEndpoint(token, null, new FakeMagicLinkSignInManager());

        var redirectResult = Assert.IsType<RedirectHttpResult>(actual);
        Assert.Equal("/Account/BadMagic", redirectResult.Url);
        
        navigationManager.NavigateTo(redirectResult.Url);

        var cut = this.RenderComponent<Magic>();
        Assert.NotNull(cut.Find("#invalid-magic-link-token-alert"));
    }

    [Fact]
    public async Task Redirects_to_ReturnUrl_when_token_is_valid()
    {
        var token = FakeMagicLinkSignInManager.SuccessToken;
        var targetUrl = "/Organization/Settings";
        
        var actual = await ComplimentaryEndpoints.AccountMagicEndpoint(token, targetUrl, new FakeMagicLinkSignInManager());
        
        var redirectResult = Assert.IsType<RedirectHttpResult>(actual);
        Assert.Equal(targetUrl, redirectResult.Url);
    }
    
    [Fact]
    public async Task Redirects_to_default_url_when_token_is_valid_but_external_return_url()
    {
        var token = FakeMagicLinkSignInManager.SuccessToken;
        var targetUrl = "https://google.com";
        
        var actual = await ComplimentaryEndpoints.AccountMagicEndpoint(token, targetUrl, new FakeMagicLinkSignInManager());
        
        var redirectResult = Assert.IsType<RedirectHttpResult>(actual);
        Assert.True(redirectResult.AcceptLocalUrlOnly);
         // Note: You can't directly test the URL validation here since it happens during execution
        // You would need integration tests to verify the actual redirect behavior
    }
}