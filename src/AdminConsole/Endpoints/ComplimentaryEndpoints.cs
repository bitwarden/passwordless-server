using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Authenticators;
using Stripe;
using Stripe.Checkout;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.AdminConsole.Endpoints;

public static class ComplimentaryEndpoints
{
    public static void MapComplimentaryEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/Account/Magic", AccountMagicEndpoint);
        builder.MapPost("/app/{appId}/settings/authenticators/manage/api", ManageAuthenticatorAsync)
            .RequireAuthorization(CustomPolicy.HasAppRole);
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

        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/Organization/Overview";



        // Only allow the url to be a relative url, to prevent open redirect attacks
        return LocalRedirect(returnUrl);
    }

    public static async Task<IResult> ManageAuthenticatorAsync(
        [FromRoute] string appId,
        [FromForm] AuthenticatorManagementRequest request,
        [FromServices] IScopedPasswordlessClient passwordlessClient,
        HttpContext context)
    {
        try
        {
            switch (request.Action?.ToLowerInvariant())
            {
                case "add":
                {
                    var addRequest = new AddAuthenticatorsRequest(request.Selected, true);
                    await passwordlessClient.AddAuthenticatorsAsync(addRequest);
                    break;
                }
                case "remove":
                {
                    var removeRequest = new RemoveAuthenticatorsRequest(request.Selected);
                    await passwordlessClient.RemoveAuthenticatorsAsync(removeRequest);
                    break;
                }
                default:
                    return BadRequest(new { error = "Invalid action. Must be 'add' or 'remove'." });
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Error managing authenticator",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    public class AuthenticatorManagementRequest
    {
        public Guid[] Selected { get; set; } = Array.Empty<Guid>();
        public string? Action { get; set; }
    }
}