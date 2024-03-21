using System.Data;
using Passwordless.Common.Background;
using Passwordless.Service;

namespace Passwordless.Api.Reporting.Background;

public sealed class PeriodicCredentialReportsBackgroundService : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PeriodicCredentialReportsBackgroundService(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<PeriodicCredentialReportsBackgroundService> logger) : base(
        new TimeOnly(22, 00, 00),
        TimeSpan.FromDays(1),
        timeProvider,
        logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(PeriodicCredentialReportsBackgroundService)} is executing.");
        using IServiceScope scope = _serviceProvider.CreateScope();
        var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();
        int result;
        try
        {
            result = await reportingService.UpdatePeriodicCredentialReportsAsync();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error updating periodic credential reports.");
            return;
        }
        Logger.LogInformation("{BackgroundService} updated {Records}.", nameof(PeriodicCredentialReportsBackgroundService), result);

    }
}