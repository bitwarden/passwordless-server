using Microsoft.Extensions.Options;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/throw/api", (ctx) => throw new ApiException("test_error", "Testing error response", 400));
        app.MapGet("health/throw/exception", (ctx) => throw new Exception("Testing error response", new Exception("Inner exception")));
        app.MapGet("health/test", (IOptionsSnapshot<MailConfiguration> options) =>
        {
            return options.Value.Providers.Count > 0 ? Results.Ok() : Results.NotFound();
        });

    }
}