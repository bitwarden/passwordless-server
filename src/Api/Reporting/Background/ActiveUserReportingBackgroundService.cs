using System.Data;
using Passwordless.Common.Background;
using Passwordless.Service;

namespace Passwordless.Api.Reporting.Background;

public sealed class ActiveUserReportingBackgroundService : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ActiveUserReportingBackgroundService(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<ActiveUserReportingBackgroundService> logger) : base(
        new TimeSpan(22, 0, 0),
        TimeSpan.FromMinutes(1),
        timeProvider,
        logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(ActiveUserReportingBackgroundService)} is executing.");
        using IServiceScope scope = _serviceProvider.CreateScope();
        var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();
        int result;
        try
        {
            result = await reportingService.UpdatePeriodicActiveUserReportsAsync();
        }
        catch (DBConcurrencyException)
        {
            return;
        }
        Logger.LogInformation("{BackgroundService} updated {Records}.", nameof(ActiveUserReportingBackgroundService), result);
    }
}