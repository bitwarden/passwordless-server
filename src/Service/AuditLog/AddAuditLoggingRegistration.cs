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
            .AddScoped(GetAuditLogger);

    private static IAuditLogger GetAuditLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IAuditLogContext>().Features.AuditLoggingIsEnabled
            ? serviceProvider.GetRequiredService<AuditLoggerEfWriteStorage>()
            : NoOpAuditLogger.Instance;
}