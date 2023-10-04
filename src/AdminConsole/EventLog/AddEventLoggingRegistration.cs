using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.EventLog;

public static class AddEventLoggingRegistration
{
    public static IServiceCollection AddEventLogging(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddScoped<IEventLoggerStorage, EventLoggerEfReadStorage>()
            .AddScoped<EventLoggerEfWriteStorage>()
            .AddScoped<EventLoggerEfUnauthenticatedWriteStorage>()
            .AddScoped(GetEventLogger)
            .AddTransient<IEventLogService, EventLogService>();

    private static IEventLogger GetEventLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<ICurrentContext>() switch
        {
            { OrgId: null } => serviceProvider.GetRequiredService<EventLoggerEfUnauthenticatedWriteStorage>(),
            { OrganizationFeatures.EventLoggingIsEnabled: true } => serviceProvider.GetRequiredService<EventLoggerEfWriteStorage>(),
            _ => NoOpEventLogger.Instance
        };
}