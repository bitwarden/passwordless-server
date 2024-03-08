using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Background;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog;

public sealed class EventDeletionBackgroundWorker : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public EventDeletionBackgroundWorker(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<EventDeletionBackgroundWorker> logger) : base(
        new TimeOnly(0, 0, 0),
        TimeSpan.FromDays(1),
        timeProvider,
        logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        try
        {
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<DbGlobalContext>();

            var eventsToDelete = await dbContext.ApplicationEvents
                .Where(x => x.PerformedAt <= TimeProvider.GetUtcNow().UtcDateTime.AddDays(-x.Application.Features.EventLoggingRetentionPeriod))
                .ExecuteDeleteAsync(cancellationToken);

            Logger.LogInformation("{eventsDeleted} events deleted from EventLogging", eventsToDelete);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Event Log Deletion Worker was cancelled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Event Log Deletion failed.");
        }
    }
}