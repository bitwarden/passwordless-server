using Microsoft.AspNetCore.Authorization;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Authorization;

public class HasAppHandler : AuthorizationHandler<HasAppRoleRequirement>
{
    private readonly ConsoleDbContext _dbContext;

    public HasAppHandler(ConsoleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

        var organizationId = httpContext.User.GetOrgId();

        if (!organizationId.HasValue)
        {
            return false;
        }

        var hasAppId = httpContext.GetRouteData().Values.TryGetValue(RouteParameters.AppId, out var appIdObj);
        if (!hasAppId)
        {
            return false;
        }

        var appId = appIdObj!.ToString();

        return _dbContext.Applications.Any(x => x.OrganizationId == organizationId.Value && x.Id == appId);
    }
}