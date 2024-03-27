namespace Passwordless.AdminConsole.RateLimiting;

public static class AddRateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(limiter =>
        {
            // Reject with 429 instead of the default 503
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.AddAdminPageRateLimitPolicy();
        });
}