using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Features;

namespace Passwordless.Service.AuditLog;

public static class AddAuditLoggingRegistration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddScoped<IAuditLogStorage, AuditLoggerEfStorage>()
            .AddScoped<AuditLoggerEfStorage>()
            .AddScoped(GetAuditLogger);

    private static async Task<IAuditLogger> GetAuditLogger(IServiceProvider serviceProvider) =>
        (await serviceProvider.GetRequiredService<IFeatureContextProvider>().UseContext()).AuditLoggingIsEnabled
            ? serviceProvider.GetRequiredService<AuditLoggerEfStorage>()
            : NoOpAuditLogger.Instance;
}