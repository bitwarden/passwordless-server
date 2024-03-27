using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Endpoints;

namespace Passwordless.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            // Rate limiter terminates the request chain instead of throwing an exception,
            // so the error will not reach the exception handler middleware.
            // As a workaround, we provide some minimally empathetic feedback to the user here.
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var problemDetailsContext = new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = context.HttpContext.Response.StatusCode,
                        Title = "Too many requests",
                        Type =
                            $"https://docs.passwordless.dev/guide/errors.html#{context.HttpContext.Response.StatusCode}",
                        Extensions =
                        {
                            ["retryAfter"] = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                                ? retryAfter.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                                : null
                        }
                    }
                };

                using var scope = context.HttpContext.RequestServices.CreateScope();
                var problemDetailsService = scope.ServiceProvider.GetRequiredService<IProblemDetailsService>();

                await problemDetailsService.WriteAsync(problemDetailsContext);
            };

            options.AddMagicRateLimiterPolicy();
        });
}