using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Authorization;

public class HasAppHandler : AuthorizationHandler<HasAppRoleRequirement>
{
    private readonly ConsoleDbContext _dbContext;
    private readonly ILogger<HasAppHandler> _logger;

    public HasAppHandler(
        ConsoleDbContext dbContext,
        ILogger<HasAppHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAppRoleRequirement requirement)
    {
        _logger.LogInformation("Checking if user has app in tenant - START");
        if (await HasAppInTenant(context))
        {
            context.Succeed(requirement);
        }
        _logger.LogInformation("Checking if user has app in tenant - END");

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