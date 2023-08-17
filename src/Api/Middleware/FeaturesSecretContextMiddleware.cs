using Passwordless.Common.Parsers;
using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Middleware;

public class FeaturesSecretContextMiddleware
{
    private readonly RequestDelegate _next;
    public const string AuthenticationType = "ApiSecret";

    public FeaturesSecretContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IFeaturesContext featuresContext, ITenantStorageFactory storageFactory)
    {
        if (httpContext?.User.Identity?.AuthenticationType != AuthenticationType)
        {
            await _next(httpContext);
            return;
        }
        if (!httpContext.Request.RouteValues.ContainsKey("ApiSecret"))
        {
            await _next(httpContext);
            return;
        }
        var apiSecret = httpContext.Request.Headers["ApiSecret"].ToString();
        var appId = ApiKeyParser.GetAppId(apiSecret);
        var storage = storageFactory.Create(appId);
        var features = await storage.GetAppFeaturesAsync();
        if (features != null)
        {
            featuresContext = new FeaturesContext(features.AuditLoggingIsEnabled, features.AuditLoggingRetentionPeriod);
        }
        await _next(httpContext);
    }
}