using Microsoft.AspNetCore.Identity;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Controller;

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
        return endpoints;
    }
}