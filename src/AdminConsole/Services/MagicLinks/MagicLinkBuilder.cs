using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Passwordless.AdminConsole.Identity;
using Passwordless.Models;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkBuilder(
    SignInManager<ConsoleAdmin> signInManager,
    IPasswordlessClient passwordlessClient,
    IActionContextAccessor actionContextAccessor,
    IUrlHelperFactory urlHelperFactory,
    ILogger<MagicLinkBuilder> logger)
    : IMagicLinkBuilder
{
    public async Task<string> GetLinkAsync(string email, string? returnUrl = null)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError("User not found: {email}", email);
            throw new ArgumentException("User not found", nameof(email));
        }

        var token = await passwordlessClient.GenerateAuthenticationTokenAsync(new AuthenticationOptions(user.Id));
        var urlBuilder = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext ?? throw new InvalidOperationException("ActionContext is null"));
        var url = urlBuilder.PageLink("/Account/Magic", values: new { returnUrl, token }) ?? urlBuilder.Content("~/");

        return url;
    }

    public string GetUrlTemplate(string? returnUrl = null)
    {
        var urlBuilder = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext ?? throw new InvalidOperationException("ActionContext is null"));

        var url = urlBuilder.PageLink("/Account/Magic", values: new { token = "$token" }) ?? urlBuilder.Content("~/");

        return url;
    }
}