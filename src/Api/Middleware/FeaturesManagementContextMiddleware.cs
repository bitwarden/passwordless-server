using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Middleware;

public class FeaturesManagementContextMiddleware
{
    private readonly RequestDelegate _next;
    public const string AuthenticationType = "ManagementKey";

    public FeaturesManagementContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IMessageWriter is injected into InvokeAsync
    public async Task InvokeAsync(HttpContext httpContext, IFeaturesContext featuresContext, ITenantStorageFactory storageFactory)
    {
        if (httpContext?.User.Identity?.AuthenticationType != AuthenticationType)
        {
            await _next(httpContext);
            return;
        }
        if (!httpContext.Request.RouteValues.ContainsKey("appId"))
        {
            await _next(httpContext);
            return;
        }
        var appId = httpContext.Request.RouteValues["appId"].ToString();
        var storage = storageFactory.Create(appId);
        var features = await storage.GetAppFeaturesAsync();
        if (features != null)
        {
            featuresContext = new FeaturesContext(features.AuditLoggingIsEnabled, features.AuditLoggingRetentionPeriod);
        }
        await _next(httpContext);
    }
}