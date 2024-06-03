using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Account");

        // The Blazor SSR sample uses the same solution for signing out, but we do not want to use all the endpoints.
        group.MapPost("/Logout", async (SignInManager<ConsoleAdmin> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/");
        }).RequireAuthorization();

        group.MapPost("/StepUp",
            async (
                IOptions<PasswordlessOptions> options,
                HttpContext context,
                StepUpPurpose purpose,
                [FromBody] StepUpRequest request) =>
            {
                var http = new HttpClient
                {
                    BaseAddress = new Uri(options.Value.ApiUrl),
                    DefaultRequestHeaders = { { "ApiSecret", options.Value.ApiSecret } }
                };

                using var response = await http.PostAsJsonAsync("/signin/verify", new
                {
                    Token = request.StepUpToken,
                    Purpose = request.Purpose
                });

                var identity = (ClaimsIdentity)context.User.Identity!;
                var existingStepUpClaim = identity.FindFirst(request.Purpose);

                if (existingStepUpClaim != null)
                {
                    identity.RemoveClaim(existingStepUpClaim);
                }
                identity.AddClaim(new Claim(request.Purpose, DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)).ToString(CultureInfo.CurrentCulture)));

                purpose.Purpose = string.Empty;

                await context.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));

                return Results.Redirect(request.ReturnUrl);
            }).RequireAuthorization();
        
        return endpoints;
    }
    
    record StepUpRequest(string StepUpToken, string ReturnUrl, string Purpose);
}