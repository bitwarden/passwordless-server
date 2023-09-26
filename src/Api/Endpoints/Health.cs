using Passwordless.Service.Helpers;

namespace Passwordless.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/throw/api", (ctx) => throw new ApiException("test_error", "Testing error response", 400));
        app.MapGet("health/throw/exception", (ctx) => throw new Exception("Testing error response", new Exception("Inner exception")));
    }
}