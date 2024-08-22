using Passwordless.Common.HealthChecks;

namespace Passwordless.Api.Middleware;

public static class PathValidation
{
    private const string RootPath = "/";

    public static Func<HttpContext, bool> ShouldRunEventLogMiddleware = o =>
    {
        // If the incoming request is the root, we don't want to execute this middleware.
        if (o.Request.Path == RootPath)
        {
            return false;
        }

        // If the incoming request is the root, we don't want to execute this middleware.
        if (o.GetEndpoint() == null)
        {
            return false;
        }

        // Ignore health check endpoints
        return !o.Request.Path.StartsWithSegments(HealthCheckEndpoints.Path);
    };
}