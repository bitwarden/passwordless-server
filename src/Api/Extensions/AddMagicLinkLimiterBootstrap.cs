using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Passwordless.Service.Models;

namespace Passwordless.Api.Extensions;

public static class AddMagicLinkLimiterBootstrap
{
    public const string MagicLinkRateLimiterPolicyName = "MagicLinkLimiter";

    public static IServiceCollection AddMagicLinks(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddDistributedMemoryCache()
            .AddMagicLinksLimiter();

    public static IServiceCollection AddMagicLinksLimiter(this IServiceCollection serviceCollection) =>
        serviceCollection.AddRateLimiter(limiters =>
            limiters.AddPolicy(MagicLinkRateLimiterPolicyName, context =>
                new RateLimitPartition<string>(context.Request.GetTenantName() ?? string.Empty, appId =>
                    context.RequestServices.GetRequiredService<MagicLinkRateLimiterProvider>().GetRateLimiter(appId))));
}

public class ApplicationRiskMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, MagicLinkRateLimiterProvider provider)
    {
        var tenantId = context.Request.GetTenantName();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // get risk from database (set by background service)

            await provider.SetRateLimiterAsync(tenantId, 0);
        }

        await next(context);
    }
}

public class MagicLinkRateLimiterProvider
{
    private readonly IDistributedCache _cache;

    public MagicLinkRateLimiterProvider(IDistributedCache cache)
    {
        _cache = cache;
    }

    public RateLimiter GetRateLimiter(string appId)
    {
        var applicationRiskByteArray = _cache.Get($"magic-link-{appId}");

        if (applicationRiskByteArray == null)
            return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()); // we'll return the default rate limiting...maybe even the 0 trust one

        var applicationRisk = Deserialize(applicationRiskByteArray);

        return new FixedWindowRateLimiter(GetOptions(applicationRisk));
    }

    private FixedWindowRateLimiterOptions GetOptions(ApplicationRisk risk) => risk.Risk switch
    {
        < 10 and >= 0 => new FixedWindowRateLimiterOptions(),
        < 20 and >= 10 => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromSeconds(60) }, // 10000 emails/month 120/min
        < 30 and >= 20 => new FixedWindowRateLimiterOptions(),
        < 40 and >= 30 => new FixedWindowRateLimiterOptions(),
        < 50 and >= 40 => new FixedWindowRateLimiterOptions(),
        < 60 and >= 50 => new FixedWindowRateLimiterOptions(),
        < 70 and >= 60 => new FixedWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(60) }, // 100 emails/month 2 emails/min
        < 80 and >= 70 => new FixedWindowRateLimiterOptions(),
        < 90 and >= 80 => new FixedWindowRateLimiterOptions { PermitLimit = 1, Window = TimeSpan.FromSeconds(60) }, // 10 emails/month 1 email/minute
        < 100 and >= 90 => new FixedWindowRateLimiterOptions(), // have to think about this one (its limited by destination)
        _ => new FixedWindowRateLimiterOptions() // override the on reject to return 200 but do nothing
    };

    public Task SetRateLimiterAsync(string appId, int risk) =>
        _cache.SetAsync($"magic-link-{appId}",
            Serialize(new ApplicationRisk { Tenant = appId, Risk = risk }),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(1) });

    private byte[] Serialize(ApplicationRisk dto) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

    private ApplicationRisk Deserialize(byte[] byteArray) => JsonSerializer.Deserialize<ApplicationRisk>(Encoding.UTF8.GetString(byteArray)) ?? new ApplicationRisk();
}