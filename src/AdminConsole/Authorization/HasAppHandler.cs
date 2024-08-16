using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAppRoleRequirement requirement)
    {
        if (await HasAppInTenant(context))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> HasAppInTenant(AuthorizationHandlerContext context)
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

        return await _dbContext.Applications.AnyAsync(x => x.OrganizationId == organizationId.Value && x.Id == appId);
    }
}