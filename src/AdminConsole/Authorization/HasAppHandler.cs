using Microsoft.AspNetCore.Authorization;

namespace AdminConsole.Authorization;

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
        var gotApp = httpContext.GetRouteData().Values.TryGetValue("app", out var app);
        if (!gotApp)
        {
            return false;
        }

        string appId = app.ToString();

        return context.User.HasClaim(c => c.Type == "AppId" && c.Value == appId);
    }
}