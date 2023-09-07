using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef.AuditLog;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddScoped<IAuditLogStorage, AuditLoggerEfStorage>()
            .AddScoped<AuditLoggerEfStorage>()
            .AddScoped(GetAuditLogger)
            .AddAuditDatabase(configuration);

    private static async Task<IAuditLogger> GetAuditLogger(IServiceProvider serviceProvider) =>
        (await serviceProvider.GetRequiredService<IFeatureContextProvider>().UseContext()).AuditLoggingIsEnabled
            ? serviceProvider.GetRequiredService<AuditLoggerEfStorage>()
            : NoOpAuditLogger.Instance;

    private static IServiceCollection AddAuditDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var auditSqlite = configuration.GetConnectionString("sqlite:audit");
        var auditMsSql = configuration.GetConnectionString("mssql:audit");

        if (!string.IsNullOrEmpty(auditSqlite))
        {
            services.AddDbContext<DbAuditLogContext, DbAuditSqliteContext>((provider, builder) =>
            {
                builder.UseSqlite(provider.GetRequiredService<IConfiguration>().GetConnectionString("sqlite:audit"));
            });
        }

        if (!string.IsNullOrEmpty(auditMsSql))
        {
            services.AddDbContext<DbAuditLogContext, DbAuditMsSqlContext>((provider, builder) =>
            {
                builder.UseSqlServer(provider.GetRequiredService<IConfiguration>().GetConnectionString("mssql:audit"));
            });
        }

        return services;
    }
}