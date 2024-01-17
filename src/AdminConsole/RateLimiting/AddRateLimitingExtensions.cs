namespace Passwordless.AdminConsole.RateLimiting;

public static class AddRateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(limiter => limiter.AddAdminPageRateLimitPolicy());
}