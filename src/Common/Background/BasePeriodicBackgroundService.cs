namespace Passwordless.Common.Background;

public abstract class BasePeriodicBackgroundService : BackgroundService
{
    protected ILogger Logger;

    /// <summary>
    /// The time of day when the service should run.
    /// </summary>
    private readonly TimeSpan _executionTime;

    /// <summary>
    /// The period of time between executions.
    /// </summary>
    private readonly TimeSpan _period;

    private ITimer? _timer;

    private Task? _executingTask;

    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="executionTime">The time of day when the service should run.</param>
    /// <param name="period">The period of time between executions.</param>
    /// <param name="timeProvider"></param>
    protected BasePeriodicBackgroundService(
        TimeSpan executionTime,
        TimeSpan period,
        TimeProvider timeProvider,
        ILogger logger)
    {
        _executionTime = executionTime;
        _period = period;
        _timeProvider = timeProvider;
        Logger = logger;
    }

    protected CancellationTokenSource CancellationToken { get; } = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var executionPlan = ExecutionPlanUtility.GetExecutionPlan(_executionTime, _period, _timeProvider);
        Logger.LogInformation("Starting {BackgroundService} in {InitialDelay}.", GetType().Name, executionPlan.InitialDelay);
        _timer = new Timer(DoWork, null, executionPlan.InitialDelay, _period);

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _executingTask = DoWorkAsync(CancellationToken.Token);
    }

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Prevent from spawning new workers
        _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        if (_executingTask == null)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        try
        {
            // Signal the worker to cancel
            await CancellationToken.CancelAsync();
        }
        finally
        {
            if (_executingTask != null)
            {
                // Wait until the worker completes or the stop token triggers
                await Task.WhenAny(_executingTask!, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);

        _timer?.Dispose();
        CancellationToken.Dispose();
    }
}