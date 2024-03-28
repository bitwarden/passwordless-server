using System.Globalization;
using System.Threading.RateLimiting;

namespace Passwordless.AdminConsole.RateLimiting;

public static class AddRateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(limiter =>
        {
            // Rate limiter terminates the request chain instead of throwing an exception,
            // so the error will not reach the exception handler middleware.
            // As a workaround, we provide some minimally empathetic feedback to the user here.
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfterFormatted = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? retryAfter.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                    : "a few";

                await context.HttpContext.Response.WriteAsync(
                    $"Too many requests. Please try again after {retryAfterFormatted} second(s).",
                    cancellationToken
                );
            };

            limiter.AddAdminPageRateLimitPolicy();
        });
}