using Microsoft.AspNetCore.Http;
using Passwordless.Common.Utils;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Features;

public class FeatureContextProvider : IFeatureContextProvider
{
    private readonly Lazy<Task<IFeaturesContext>> _lazyContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantStorageFactory _storageFactory;

    public FeatureContextProvider(
        IHttpContextAccessor httpContextAccessor,
        ITenantStorageFactory storageFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _storageFactory = storageFactory;
        this._lazyContext = new Lazy<Task<IFeaturesContext>>(async () =>
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null) return new NullFeaturesContext();

            string appId = null;
            if (httpContext.Request.RouteValues.ContainsKey("appId"))
            {
                appId = httpContext.Request.RouteValues["appId"].ToString();
            }
            if (httpContext.Request.Headers.ContainsKey("ApiKey"))
            {
                appId = ApiKeyUtils.GetAppId(httpContext.Request.Headers["ApiKey"].ToString());
            }
            if (httpContext.Request.Headers.ContainsKey("ApiSecret"))
            {
                appId = ApiKeyUtils.GetAppId(httpContext.Request.Headers["ApiSecret"].ToString());
            }

            if (string.IsNullOrWhiteSpace(appId)) return new NullFeaturesContext();

            var storage = _storageFactory.Create(appId);
            var features = await storage.GetAppFeaturesAsync();
            if (features != null)
            {
                return new FeaturesContext(
                    features.EventLoggingIsEnabled,
                    features.EventLoggingRetentionPeriod,
                    features.DeveloperLoggingEndsAt,
                    features.MaxUsers,
                    features.AllowAttestation,
                    features.IsGenerateSignInTokenEndpointEnabled,
                    features.IsMagicLinksEnabled);
            }
            return new NullFeaturesContext();
        });
    }

    public async Task<IFeaturesContext> UseContext()
    {
        return await _lazyContext.Value;
    }
}