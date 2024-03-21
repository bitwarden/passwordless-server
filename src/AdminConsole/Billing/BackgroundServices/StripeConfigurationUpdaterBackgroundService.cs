using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Stripe;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

public class StripeConfigurationUpdaterBackgroundService(
    IOptionsMonitor<BillingOptions> OptionsMonitor) : BackgroundService
{
    private IDisposable? _onChangeListener;
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StripeConfiguration.ApiKey = OptionsMonitor.CurrentValue.ApiKey;
        _onChangeListener = OptionsMonitor.OnChange(OnOptionsChanged);
        return Task.CompletedTask;
    }

    private void OnOptionsChanged(BillingOptions options, string? arg2)
    {
        StripeConfiguration.ApiKey = options.ApiKey;
    }

    public override void Dispose()
    {
        _onChangeListener?.Dispose();
        base.Dispose();
    }
}