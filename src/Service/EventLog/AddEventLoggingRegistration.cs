using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog;

public static class AddEventLoggingRegistration
{
    public static IServiceCollection AddEventLogging(this IServiceCollection services) =>
        services
            .AddScoped<EventLoggerEfWriteStorage>()
            .AddScoped<IEventLogStorage, EventLoggerEfReadStorage>()
            .AddScoped<IEventLogContext, EventLogContext>()
            .AddScoped(GetEventLogger)
            .AddHostedService<EventDeletionBackgroundWorker>();

    private static IEventLogger GetEventLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IEventLogContext>().Features.EventLoggingIsEnabled
            ? serviceProvider.GetRequiredService<EventLoggerEfWriteStorage>()
            : NoOpEventLogger.Instance;
}