using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog;

public static class AuditLoggingExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services) =>
        services
            .AddTransient<IAuditLogService, AuditLogService>();
}