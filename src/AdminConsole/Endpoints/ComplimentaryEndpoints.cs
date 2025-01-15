using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;
using Stripe;
using Stripe.Checkout;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.AdminConsole.Endpoints;

public static class ComplimentaryEndpoints
{
    public static void MapComplimentaryEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/Account/Magic", AccountMagicEndpoint);
    }

    /// <summary>
    /// Processes the Stripe webhook notification.
    /// </summary>
    public static async Task<IResult> AccountMagicEndpoint(
        [FromQuery] string? token,
        [FromQuery] string? returnUrl,
        [FromServices] MagicLinkSignInManager<ConsoleAdmin> signInManager)
    {
        if (string.IsNullOrEmpty(token))
        {
            // Redirect to the /Account/BadMagic page
            return Redirect("/Account/BadMagic");
        }

        var res = await signInManager.PasswordlessSignInAsync(token, true);

        if (!res.Succeeded)
        {
            // Redirect to the /Account/BadMagic page
            return Redirect("/Account/BadMagic");
        }

        returnUrl ??= "/Organization/Overview";
        
        

        // Only allow the url to be a relative url, to prevent open redirect attacks
        return LocalRedirect(returnUrl);
    }
}