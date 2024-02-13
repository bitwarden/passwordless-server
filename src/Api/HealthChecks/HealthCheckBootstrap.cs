using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Passwordless.Common.HealthChecks;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.HealthChecks;

public static class HealthCheckBootstrap
{
    private const string TagSimple = "simple";
    private const string TagDatabase = "db";
    private const string TagVersion = "version";
    private const string TagMail = "mail";

    public static void AddPasswordlessHealthChecks(this WebApplicationBuilder builder)
    {
        var healthCheckBuilder = builder.Services.AddHealthChecks();

        healthCheckBuilder.AddCheck<SimpleHealthCheck>(
            "http",
            tags: new[] { TagSimple });

        builder.Services.AddHealthChecks().AddDbContextCheck<DbGlobalContext>(
            "db",
            tags: new[] { TagDatabase });

        healthCheckBuilder.AddCheck<VersionHealthCheck>(
            "version",
            tags: new[] { TagVersion });

        var mail = builder.Configuration.GetSection("Mail");
        if (mail.GetSection("MailKit").Exists())
        {
            healthCheckBuilder.AddCheck<MailKitHealthCheck>(
                "smtp",
                tags: new[] { TagMail });
        }
    }

    public static void MapPasswordlessHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/http", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(TagSimple)
        });
        app.MapHealthChecks("/health/storage", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync,
            Predicate = registration => registration.Tags.Contains(TagDatabase)
        });
        app.MapHealthChecks("/health/version", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync,
            Predicate = registration => registration.Tags.Contains(TagVersion)
        });
        app.MapHealthChecks("/health/mail", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync,
            Predicate = registration => registration.Tags.Contains(TagMail)
        });
    }
}