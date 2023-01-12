using Service.Helpers;
using Service.Storage;
using System.Diagnostics;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("health/storage", async (HttpContext ctx, HttpRequest req) =>
        {
            var sw = Stopwatch.StartNew();
            var s = new TableStorage("health", app.Configuration);

            await s.GetAccountInformation();

            sw.Stop();
            app.Logger.LogInformation("event=health_storage latency={latency}",sw.ElapsedMilliseconds);
            return Results.Text("Took: " + sw.ElapsedMilliseconds);
        }).RequireCors("default");


        app.MapGet("health/http", (HttpContext ctx, HttpRequest req) =>
            {
                try
                {
                    app.Logger.LogInformation("event=health_http");
                    return Results.Ok();
                }
                catch (ApiException e)
                {
                    return ErrorHelper.FromException(e);
                }
            }).RequireCors("default");


        app.MapGet("account/health/http", (HttpContext ctx, HttpRequest req) =>
            {
                try
                {
                    app.Logger.LogInformation("/account/health/http called");
                    return Results.Ok();
                }
                catch (ApiException e)
                {
                    return ErrorHelper.FromException(e);
                }
            }).RequireCors("default");
    }
}
