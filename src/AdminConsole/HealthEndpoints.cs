using System.Diagnostics;
using System.Reflection;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Extensions;

namespace Passwordless.AdminConsole;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/http", () => Results.Text("Ok"));

        app.MapGet("health/storage", async (IDataService dataService) =>
        {
            var sw = Stopwatch.StartNew();
            if (!await dataService.CanConnectAsync())
            {
                return Results.StatusCode(503);
            }
            sw.Stop();
            app.Logger.LogInformation("health_storage took {Latency}", sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        });

        app.MapGet("health/api", async (IPasswordlessClient client) =>
        {
            var sw = Stopwatch.StartNew();
            await client.ListUsersAsync();
            sw.Stop();
            app.Logger.LogInformation("health_api took {Latency}", sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        });

        app.MapGet("health/throw/exception", _ =>
            throw new Exception("Testing error response", new Exception("Inner exception"))
        );

        app.MapGet("health/version", () =>
        {
            var assembly = Assembly.GetExecutingAssembly();

            return Results.Json(
                assembly.GetInformationalVersion() ??
                assembly.GetName().Version?.ToString()
            );
        });
    }
}