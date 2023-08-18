using Passwordless.Common.Parsers;
using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Middleware;

public class FeaturesApiKeyContextMiddleware
{
    private readonly RequestDelegate _next;
    public const string PublicAuthenticationType = "ApiKey";
    public const string PrivateAuthenticationType = "ApiSecret";

    public FeaturesApiKeyContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IFeaturesContext featuresContext, ITenantStorageFactory storageFactory)
    {
        if (httpContext?.User.Identity?.AuthenticationType != PublicAuthenticationType
            && httpContext?.User.Identity?.AuthenticationType != PrivateAuthenticationType)
        {
            await _next(httpContext);
            return;
        }
        string headerName;
        if (httpContext.Request.Headers.ContainsKey(PublicAuthenticationType))
        {
            headerName = PublicAuthenticationType;
        }
        else if (httpContext.Request.Headers.ContainsKey(PrivateAuthenticationType))
        {
            headerName = PrivateAuthenticationType;
        }
        else
        {
            await _next(httpContext);
            return;
        }
        var apiKey = httpContext.Request.Headers[headerName].ToString();
        var appId = ApiKeyParser.GetAppId(apiKey);
        var storage = storageFactory.Create(appId);
        var features = await storage.GetAppFeaturesAsync();
        if (features != null)
        {
            featuresContext = new FeaturesContext(features.AuditLoggingIsEnabled, features.AuditLoggingRetentionPeriod);
        }
        await _next(httpContext);
    }
}