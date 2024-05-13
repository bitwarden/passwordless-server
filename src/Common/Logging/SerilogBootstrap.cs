using Serilog;
using Serilog.Sinks.Datadog.Logs;

namespace Passwordless.Common.Logging;

public static class SerilogBootstrap
{
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        var passthrough = !builder.Environment.IsProduction();
        builder.Host.UseSerilog((ctx, sp, config) =>
        {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console();

            // if (!builder.Environment.IsProduction())
            // {
            //     config.WriteTo.Seq("http://localhost:5341");
            // }

            var ddApiKey = Environment.GetEnvironmentVariable("DD_API_KEY");
            if (!string.IsNullOrEmpty(ddApiKey))
            {
                var ddSite = Environment.GetEnvironmentVariable("DD_SITE") ?? "datadoghq.eu";
                var ddUrl = $"https://http-intake.logs.{ddSite}";
                var ddConfig = new DatadogConfiguration(ddUrl);

                if (!string.IsNullOrEmpty(ddApiKey))
                {
                    config.WriteTo.DatadogLogs(
                        ddApiKey,
                        configuration: ddConfig);
                }
            }
        },
            writeToProviders: passthrough);
    }

    /// <summary>
    /// Configuring the Serilog middleware for request logging.
    /// </summary>
    /// <param name="withUserAgent">The 'UserAgent' property will contain the value of the 'User-Agent' header when set to `true`.</param>
    public static void UseSerilog(this WebApplication app, bool withUserAgent = false)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (context, httpContext) =>
            {
                if (withUserAgent)
                {
                    context.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                }
            };
        });
    }
}