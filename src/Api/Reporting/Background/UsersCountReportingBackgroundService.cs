using Passwordless.Common;
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
        new TimeSpan(22, 0, 0),
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
        var result = await reportingService.UpdatePeriodicCredentialReportsAsync();
        _logger.LogInformation("{BackgroundService} updated {Records}.", nameof(PeriodicCredentialReportsBackgroundService), result);
    }
}