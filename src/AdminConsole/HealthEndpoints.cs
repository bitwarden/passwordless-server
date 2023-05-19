using System.Diagnostics;
using System.Reflection;
using AdminConsole.Db;
using Microsoft.EntityFrameworkCore;
using Passwordless.Net;

namespace Passwordless.AdminConsole;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/http", (HttpContext ctx, HttpRequest req) => Results.Text("Ok"));

        app.MapGet("health/storage", async (HttpContext ctx, HttpRequest req, ConsoleDbContext dbContext) =>
        {
            var sw = Stopwatch.StartNew();
            await dbContext.Applications.CountAsync();
            sw.Stop();
            app.Logger.LogInformation("health_storage took {Latency}", sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        });

        app.MapGet("health/api", async (HttpContext ctx, HttpRequest req, IPasswordlessClient client) =>
        {
            var sw = Stopwatch.StartNew();
            await client.ListUsers();
            sw.Stop();
            app.Logger.LogInformation("health_api took {Latency}", sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        });

        app.MapGet("health/throw/exception", (ctx) => throw new Exception("Testing error response", new Exception("Inner exception")));

        app.MapGet("health/version", (HttpContext ctx, HttpRequest req) => Results.Json(Assembly.GetExecutingAssembly().GetName().Version));
    }
}