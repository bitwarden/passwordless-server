using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Passwordless.AdminConsole.Identity;
using Passwordless.Models;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkBuilder(
    SignInManager<ConsoleAdmin> signInManager,
    IServiceProvider services,
    IActionContextAccessor actionContextAccessor,
    IUrlHelperFactory urlHelperFactory,
    ILogger<MagicLinkBuilder> logger)
    : IMagicLinkBuilder
{
    public async Task<string> GetLinkAsync(string email, string? returnUrl = null)
    {
        // Passwordless client is fetched lazily because the MagicLinkBuilder may need to instantiate
        // in a context where the API is not accessible, for example when the Admin Console has not
        // been initialized yet. Doing this prevents it from crashing unless the client is actually used.
        var passwordlessClient = services.GetRequiredService<IPasswordlessClient>();

        var user = await signInManager.UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError("User not found: {email}", email);
            throw new ArgumentException("User not found", nameof(email));
        }

        var token = await passwordlessClient.GenerateAuthenticationTokenAsync(new AuthenticationOptions(user.Id));

        var urlBuilder = urlHelperFactory.GetUrlHelper(
            actionContextAccessor.ActionContext ??
            throw new InvalidOperationException("ActionContext is null")
        );

        var url = urlBuilder.PageLink("/Account/Magic", values: new { returnUrl, token }) ?? urlBuilder.Content("~/");

        return url;
    }

    public string GetUrlTemplate(string? returnUrl = null)
    {
        var urlBuilder = urlHelperFactory.GetUrlHelper(
            actionContextAccessor.ActionContext ??
            throw new InvalidOperationException("ActionContext is null")
        );

        var url = urlBuilder.PageLink("/Account/Magic", values: new { token = "$TOKEN", returnUrl }) ??
                  urlBuilder.Content("~/");

        return url;
    }
}