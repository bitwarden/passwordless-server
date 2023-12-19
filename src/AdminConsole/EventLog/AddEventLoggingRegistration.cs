using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.EventLog;

public static class AddEventLoggingRegistration
{
    public static void AddEventLogging<TDbContext>(this IServiceCollection serviceCollection) where TDbContext : ConsoleDbContext =>
        serviceCollection
            .AddScoped<IEventLoggerStorage, EventLoggerEfReadStorage<TDbContext>>()
            .AddScoped<EventLoggerEfWriteStorage<TDbContext>>()
            .AddScoped<EventLoggerEfUnauthenticatedWriteStorage<TDbContext>>()
            .AddScoped(GetEventLogger<TDbContext>)
            .AddTransient<IInternalEventLogStorageContext, InternalEventLogStorageContext<TDbContext>>()
            .AddTransient<IEventLogService, EventLogService>()
            .AddHostedService<EventDeletionBackgroundWorker>();

    private static IEventLogger GetEventLogger<TDbContext>(IServiceProvider serviceProvider) where TDbContext : ConsoleDbContext =>
        serviceProvider.GetRequiredService<ICurrentContext>() switch
        {
            { OrgId: null } => serviceProvider.GetRequiredService<EventLoggerEfUnauthenticatedWriteStorage<TDbContext>>(),
            { OrganizationOrganizationFeatures.EventLoggingIsEnabled: true } => serviceProvider.GetRequiredService<EventLoggerEfWriteStorage<TDbContext>>(),
            _ => NoOpEventLogger.Instance
        };
}