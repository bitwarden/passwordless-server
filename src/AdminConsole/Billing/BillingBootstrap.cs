using Passwordless.AdminConsole.Billing.BackgroundServices;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Services;
using Stripe;

namespace Passwordless.AdminConsole.Billing;

public static class BillingBootstrap
{
    public static void AddBilling(this WebApplicationBuilder builder)
    {
        // TODO: Change name of this configuration path away from Stripe at some point
        builder.Services.AddOptions<BillingOptions>()
            .BindConfiguration("Stripe");

        builder.Services.AddHostedService<UserCountUpdaterBackgroundService>();

        // Todo: Improve this self-hosting story.
        if (builder.Configuration.GetValue("SelfHosted", false))
        {
            builder.Services.AddScoped<ISharedBillingService, NoOpBillingService>();
        }
        else
        {
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];
            builder.Services.AddScoped<ISharedBillingService, SharedStripeBillingService>();
            builder.Services.AddHostedService<MeteredBillingBackgroundService>();
        }
    }
}