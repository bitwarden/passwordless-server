using Passwordless.Api.Endpoints;

namespace Passwordless.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddMagicRateLimiterPolicy();
        });
}