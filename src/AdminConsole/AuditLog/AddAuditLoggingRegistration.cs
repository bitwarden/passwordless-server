using Passwordless.AdminConsole.AuditLog.Loggers;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddScoped<IAuditLoggerStorage, AuditLoggerEfReadStorage>()
            .AddScoped<AuditLoggerEfWriteStorage>()
            .AddScoped<AuditLoggerEfUnauthenticatedWriteStorage>()
            .AddScoped(GetAuditLogger)
            .AddTransient<IAuditLogService, AuditLogService>();

    private static IAuditLogger GetAuditLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<ICurrentContext>() switch
        {
            { OrgId: null } => serviceProvider.GetRequiredService<AuditLoggerEfUnauthenticatedWriteStorage>(),
            { OrganizationFeatures.AuditLoggingIsEnabled: true } => serviceProvider.GetRequiredService<AuditLoggerEfWriteStorage>(),
            _ => NoOpAuditLogger.Instance
        };
}