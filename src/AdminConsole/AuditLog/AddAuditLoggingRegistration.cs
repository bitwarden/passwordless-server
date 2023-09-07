using Passwordless.AdminConsole.AuditLog.Loggers;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection serviceCollection, IConfiguration configuration) =>
        serviceCollection
            .AddScoped<IAuditLoggerStorage, AuditLoggerEfStorage>()
            .AddScoped<AuditLoggerEfStorage>()
            .AddScoped(GetAuditLogger)
            .AddTransient<IAuditLogService, AuditLogService>();

    private static IAuditLogger GetAuditLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<ICurrentContext>().OrganizationFeatures.AuditLoggingIsEnabled
            ? serviceProvider.GetRequiredService<AuditLoggerEfStorage>()
            : NoOpAuditLogger.Instance;
}