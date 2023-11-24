using Passwordless.AdminConsole.Billing.BackgroundServices;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Configuration;
using Stripe;

namespace Passwordless.AdminConsole.Billing;

public static class BillingBootstrap
{
    public static void AddBilling<TDbContext>(this WebApplicationBuilder builder)
        where TDbContext : ConsoleDbContext
    {
        builder.Services.AddOptions<StripeOptions>()
            .BindConfiguration("Stripe");

        builder.Services.AddHostedService<UserCountUpdaterBackgroundService>();

        // Todo: Improve this self-hosting story.
        if (builder.Configuration.IsSelfHosted())
        {
            builder.Services.AddScoped<ISharedBillingService, NoOpBillingService<TDbContext>>();
        }
        else
        {
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];
            builder.Services.AddScoped<ISharedBillingService, SharedStripeBillingService<TDbContext>>();
            builder.Services.AddHostedService<MeteredBillingBackgroundService>();
        }
    }
}