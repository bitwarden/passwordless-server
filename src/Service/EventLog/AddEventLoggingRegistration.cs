using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog;

public static class AddEventLoggingRegistration
{
    public static IServiceCollection AddEventLogging(this IServiceCollection services) =>
        services
            .AddTransient<EventLoggerEfWriteStorage>()
            .AddTransient<UnauthenticatedEventLoggerEfWriteStorage>()
            .AddTransient(GetEventLogger)
            .AddScoped<EventCache>()
            .AddScoped<IEventLogStorage, EventLoggerEfReadStorage>()
            .AddScoped<IEventLogContext, EventLogContext>()
            .AddHostedService<EventDeletionBackgroundWorker>();

    private static IEventLogger GetEventLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IEventLogContext>() switch
        {
            { IsAuthenticated: false } => serviceProvider.GetRequiredService<UnauthenticatedEventLoggerEfWriteStorage>(),
            { Features.EventLoggingIsEnabled: true } => serviceProvider.GetRequiredService<EventLoggerEfWriteStorage>(),
            _ => NoOpEventLogger.Instance
        };
}