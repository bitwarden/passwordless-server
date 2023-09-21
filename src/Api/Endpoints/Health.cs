using System.Diagnostics;
using System.Reflection;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/http", (HttpContext ctx, HttpRequest req) => Results.Text("Ok")).RequireCors("default");

        app.MapGet("health/storage", async (HttpContext ctx, HttpRequest req, ITenantStorageFactory storageFactory) =>
        {
            var sw = Stopwatch.StartNew();
            var storage = storageFactory.Create(null);
            await storage.GetAccountInformation();
            sw.Stop();
            app.Logger.LogInformation("health_storage took {Latency}", sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        }).RequireCors("default");

        app.MapGet("health/throw/api", (ctx) => throw new ApiException("test_error", "Testing error response", 400));
        app.MapGet("health/throw/exception", (ctx) => throw new Exception("Testing error response", new Exception("Inner exception")));

        app.MapGet("health/version", (HttpContext ctx, HttpRequest req) => Results.Json(Assembly.GetExecutingAssembly().GetName().Version));
    }
}