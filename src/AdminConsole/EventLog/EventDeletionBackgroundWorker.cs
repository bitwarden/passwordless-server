using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.EventLog;

public class EventDeletionBackgroundWorker : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public EventDeletionBackgroundWorker(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<EventDeletionBackgroundWorker> logger)
        : base(new TimeOnly(0), TimeSpan.FromDays(1), timeProvider, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting Event Log Deletion Worker");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var eventLogStorageContext = scope.ServiceProvider.GetRequiredService<IInternalEventLogStorageContext>();
            var result = await eventLogStorageContext.DeleteExpiredEventsAsync(cancellationToken);
            Logger.LogInformation("{BackgroundService} deleted {Records}.", nameof(EventDeletionBackgroundWorker), result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Event Log Deletion failed.");
        }
    }
}