using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Stripe;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

public class StripeConfigurationUpdaterBackgroundService(
    IOptionsMonitor<BillingOptions> OptionsMonitor,
    ILogger<StripeConfigurationUpdaterBackgroundService> Logger) : BackgroundService
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
        if (StripeConfiguration.ApiKey != options.ApiKey)
        {
            StripeConfiguration.ApiKey = options.ApiKey;
            Logger.LogInformation("Stripe API key updated.");
        }
    }

    public override void Dispose()
    {
        _onChangeListener?.Dispose();
        base.Dispose();
    }
}