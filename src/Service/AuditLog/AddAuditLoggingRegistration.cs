using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services) =>
        services
            .AddScoped<IAuditLogger, AuditLoggerEfWriteStorage>()
            .AddScoped<IAuditLogStorage, AuditLoggerEfReadStorage>()
            .AddScoped<AuditLoggerProvider>();
}