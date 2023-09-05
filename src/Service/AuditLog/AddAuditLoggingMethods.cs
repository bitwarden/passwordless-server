using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Storage.Ef.AuditLog;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingMethods
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<INullAuditLogger, NullAuditLogger>()
            .AddTransient<IAuditLogger, AuditLogger>()
            .AddTransient<IAuditLoggerFactory, AuditLoggerFactory>()
            .AddTransient<IAuditLoggerStorageFactory, AuditLoggerStorageFactory>();

        services.AddAuditDatabase(configuration);

        return services;
    }

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