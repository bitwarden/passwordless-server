using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole;

public class CurrentContext : ICurrentContext
{
#if DEBUG
    private bool _contextSet;
#endif

    public bool InAppContext { get; private set; }

    public string? AppId { get; private set; }

    public string? ApiSecret { get; private set; }

    public string? ApiKey { get; private set; }
    public bool IsFrozen { get; private set; }
    public FeaturesContext Features { get; private set; }
    public int? OrgId { get; private set; }
    public FeaturesContext OrganizationFeatures { get; private set; } = new(false, 0, null);

    public void SetApp(Application application)
    {
#if DEBUG
        Debug.Assert(!_contextSet, "Context should only be set one time per lifetime.");
        _contextSet = true;
#endif
        InAppContext = true;
        AppId = application.Id;
        ApiSecret = application.ApiSecret;
        ApiKey = application.ApiKey;
        IsFrozen = application.DeleteAt.HasValue;
    }

    public void SetFeatures(FeaturesContext context)
    {
        Features = context;
    }

    public void SetOrganization(int organizationId, FeaturesContext featuresContext)
    {
        OrgId = organizationId;
        OrganizationFeatures = featuresContext;
    }
}

public interface ICurrentContext
{
    [MemberNotNullWhen(true, nameof(AppId))]
    [MemberNotNullWhen(true, nameof(ApiSecret))]
    [MemberNotNullWhen(true, nameof(ApiKey))]
    bool InAppContext { get; }
    string? AppId { get; }
    string? ApiSecret { get; }
    string? ApiKey { get; }
    bool IsFrozen { get; }
    FeaturesContext Features { get; }
    int? OrgId { get; }
    FeaturesContext OrganizationFeatures { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetApp(Application application);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetFeatures(FeaturesContext context);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetOrganization(int organizationId, FeaturesContext context);
}

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
        ConsoleDbContext dbContext,
        IPasswordlessManagementClient passwordlessClient,
        OrganizationFeatureService organizationFeatureService)
    {
        var name = httpContext.GetRouteData();

        var hasAppRouteValue = name.Values.TryGetValue("app", out var appRouteValue);
        var appId = hasAppRouteValue && appRouteValue != null ? (string)appRouteValue : String.Empty;

        var hasOrgIdClaim = httpContext.User.HasClaim(x => x.Type == "OrgId");

        return hasOrgIdClaim
            ? InvokeCoreAsync(httpContext, appId, currentContext, dbContext, passwordlessClient, organizationFeatureService)
            : _next(httpContext);
    }

    private async Task InvokeCoreAsync(HttpContext httpContext,
        string appId,
        ICurrentContext currentContext,
        ConsoleDbContext dbContext,
        IPasswordlessManagementClient passwordlessClient, OrganizationFeatureService organizationFeatureService)
    {
        var orgId = httpContext.User.GetOrgId();
        var orgFeatures = organizationFeatureService.GetOrganizationFeatures(orgId);

#pragma warning disable CS0618 // I am the one valid caller of this method
        currentContext.SetOrganization(orgId, orgFeatures);
#pragma warning restore CS0618

        if (string.IsNullOrWhiteSpace(appId))
        {
            await _next(httpContext);
            return;
        }

        var appConfig = await dbContext.Applications.FirstOrDefaultAsync(a => a.Id == appId);

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

        var featuresContext = FeaturesContext.FromDto(features);

#pragma warning disable CS0618 // I am the one valid caller of this method
        currentContext.SetFeatures(featuresContext);
#pragma warning restore CS0618

        await _next(httpContext);
    }
}