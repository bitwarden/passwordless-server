using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Middleware;

public class CurrentContextMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Keep method non-async for non-app calls so that we can avoid the creation of a state machine when it's not needed
    public Task InvokeAsync(
        HttpContext httpContext,
        ICurrentContext currentContext,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        IOrganizationFeatureService organizationFeatureService)
    {
        var name = httpContext.GetRouteData();

        var hasAppRouteValue = name.Values.TryGetValue("app", out var appRouteValue);
        var appId = hasAppRouteValue && appRouteValue != null ? (string)appRouteValue : String.Empty;

        var hasOrgIdClaim = httpContext.User.HasClaim(x => x.Type == "OrgId");

        return hasOrgIdClaim
            ? InvokeCoreAsync(httpContext, appId, currentContext, dataService, passwordlessClient, organizationFeatureService)
            : _next(httpContext);
    }

    private async Task InvokeCoreAsync(HttpContext httpContext,
        string appId,
        ICurrentContext currentContext,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        IOrganizationFeatureService organizationFeatureService)
    {
        var orgId = httpContext.User.GetOrgId();
        var orgFeatures = organizationFeatureService.GetOrganizationFeatures(orgId.Value);
        var organization = await dataService.GetOrganizationAsync();

#pragma warning disable CS0618 // I am the one valid caller of this method
        currentContext.SetOrganization(orgId.Value, orgFeatures, organization);
#pragma warning restore CS0618

        if (string.IsNullOrWhiteSpace(appId))
        {
            await _next(httpContext);
            return;
        }

        var appConfig = await dataService.GetApplicationAsync(appId);

        if (appConfig is null)
        {
            await _next(httpContext);
            return;
        }

#pragma warning disable CS0618 // I am the one valid caller of this method
        currentContext.SetApp(appConfig);
#pragma warning restore CS0618

        var features = await passwordlessClient.GetFeaturesAsync(appId);
        if (features is null)
        {
            await _next(httpContext);
            return;
        }

        var featuresContext = ApplicationFeatureContext.FromDto(features);

#pragma warning disable CS0618 // I am the one valid caller of this method
        currentContext.SetFeatures(featuresContext);
#pragma warning restore CS0618

        await _next(httpContext);
    }
}