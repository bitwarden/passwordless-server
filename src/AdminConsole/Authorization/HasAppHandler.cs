using Microsoft.AspNetCore.Authorization;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Authorization;

public class HasAppHandler : AuthorizationHandler<HasAppRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAppRoleRequirement requirement)
    {
        if (HasAppInTenant(context))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool HasAppInTenant(AuthorizationHandlerContext context)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            return false;
        }

        // get app
        var gotApp = httpContext.GetRouteData().Values.TryGetValue(RouteParameters.AppId, out var app);
        if (!gotApp)
        {
            return false;
        }

        string appId = app.ToString();

        return context.User.HasClaim(c => c.Type == CustomClaimTypes.AppId && c.Value == appId);
    }
}