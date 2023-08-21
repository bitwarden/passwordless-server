using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AdminConsole.Db;
using AdminConsole.Models;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Models.DTOs;
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
}

public record FeaturesContext(
    bool AuditLoggingIsEnabled,
    int AuditLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt)
{
    public static FeaturesContext FromDto(AppFeatureDto dto)
    {
        return new FeaturesContext(dto.AuditLoggingIsEnabled, dto.AuditLoggingRetentionPeriod, dto.DeveloperLoggingEndsAt);
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetApp(Application application);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetFeatures(FeaturesContext context);
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
        IPasswordlessManagementClient passwordlessClient)
    {
        var name = httpContext.GetRouteData();

        if (!name.Values.TryGetValue("app", out var appRouteValue) || appRouteValue is not string appId)
        {
            return _next(httpContext);
        }

        return InvokeCoreAsync(httpContext, appId, currentContext, dbContext, passwordlessClient);
    }

    private async Task InvokeCoreAsync(
        HttpContext httpContext,
        string appId,
        ICurrentContext currentContext,
        ConsoleDbContext dbContext,
        IPasswordlessManagementClient passwordlessClient)
    {
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