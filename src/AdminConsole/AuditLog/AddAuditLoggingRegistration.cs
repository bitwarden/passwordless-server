using AdminConsole.Db.AuditLog;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.AuditLog.Loggers;
using Passwordless.AdminConsole.AuditLog.Storage;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection serviceCollection, IConfiguration configuration) =>
        serviceCollection
            .AddTransient<IAuditLoggerStorageProvider, AuditLoggerStorageProvider>()
            .AddTransient<IAuditLoggerEfStorage, AuditLoggerEfStorage>()
            .AddTransient<IOrganizationAuditLogger, OrganizationAuditLogger>()
            .AddTransient<INoOpAuditLogger, NoOpAuditLogger>()
            .AddTransient<IAuditLoggerProvider, AuditLoggerProvider>()
            .AddTransient<IAuditLogService, AuditLogService>()
            .AddAuditDatabase(configuration);

    private static IServiceCollection AddAuditDatabase(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        const string sqliteAuditKey = "sqlite:audit";

        var auditSqlite = configuration.GetConnectionString(sqliteAuditKey);

        if (!string.IsNullOrWhiteSpace(auditSqlite))
        {
            serviceCollection.AddDbContext<ConsoleAuditLogDbContext, SqliteConsoleAuditLogDbContext>((provider, builder) =>
            {
                builder.UseSqlite(provider.GetRequiredService<IConfiguration>().GetConnectionString(sqliteAuditKey));
            });
        }

        return serviceCollection;
    }
}