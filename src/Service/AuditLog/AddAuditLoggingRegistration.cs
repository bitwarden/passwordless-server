using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services) =>
        services
            .AddScoped<AuditLoggerEfWriteStorage>()
            .AddScoped<IAuditLogStorage, AuditLoggerEfReadStorage>()
            .AddScoped<IAuditLogContext, AuditLogContext>()
            .AddScoped<AuditLoggerProvider>()
            .AddScoped<IAuditLogger>(sp => 
                sp.GetRequiredService<IAuditLogContext>().Features.AuditLoggingIsEnabled
                    ? sp.GetRequiredService<AuditLoggerEfWriteStorage>()
                    : NoOpAuditLogger.Instance);
}