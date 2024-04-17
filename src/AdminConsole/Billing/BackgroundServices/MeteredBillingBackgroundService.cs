using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for synchronizing the metered seat usage with our billing partner.
/// </summary>
public sealed class MeteredBillingBackgroundService(
    IServiceProvider services,
    TimeProvider timeProvider,
    ILogger<MeteredBillingBackgroundService> logger)
    : BasePeriodicBackgroundService(new TimeOnly(23, 0, 0), TimeSpan.FromDays(1), timeProvider, logger)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = services.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<ISharedBillingService>();
            await billingService.UpdateUsageAsync();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to update billing: {error}", e.Message);
        }
    }
}