using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddScoped<IAuditLogStorage, AuditLoggerEfStorage>()
            .AddScoped<AuditLoggerEfStorage>()
            .AddScoped<AuditLoggerProvider>();
}