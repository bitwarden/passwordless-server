using System.Data;
using Passwordless.Common.Background;
using Passwordless.Service;

namespace Passwordless.Api.Reporting.Background;

public sealed class PeriodicCredentialReportsBackgroundService : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PeriodicCredentialReportsBackgroundService> _logger;

    public PeriodicCredentialReportsBackgroundService(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<PeriodicCredentialReportsBackgroundService> logger) : base(
        new TimeOnly(22, 00, 00),
        TimeSpan.FromDays(1),
        timeProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();
        int result;
        try
        {
            result = await reportingService.UpdatePeriodicCredentialReportsAsync();
        }
        catch (DBConcurrencyException)
        {
            return;
        }
        _logger.LogInformation("{BackgroundService} updated {Records}.", nameof(PeriodicCredentialReportsBackgroundService), result);

    }
}