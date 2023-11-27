using Microsoft.AspNetCore.Identity;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Controller;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Account");
        group.MapPost("/Logout", async (SignInManager<ConsoleAdmin> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/");
        });
        return endpoints;
    }
}