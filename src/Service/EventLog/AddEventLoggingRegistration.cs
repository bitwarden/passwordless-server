using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog;

public static class AddEventLoggingRegistration
{
    public static IServiceCollection AddEventLogging(this IServiceCollection services) =>
        services
            .AddScoped<EventLoggerEfWriteStorage>()
            .AddScoped<UnauthenticatedEventLoggerEfWriteStorage>()
            .AddScoped<IEventLogStorage, EventLoggerEfReadStorage>()
            .AddScoped<IEventLogContext, EventLogContext>()
            .AddScoped(GetEventLogger);

    private static IEventLogger GetEventLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IEventLogContext>() switch
        {
            { Features.EventLoggingIsEnabled: true } => serviceProvider.GetRequiredService<EventLoggerEfWriteStorage>(),
            _ => NoOpEventLogger.Instance
        };
}