using Passwordless.AdminConsole.Billing.BackgroundServices;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services;
using Stripe;

namespace Passwordless.AdminConsole.Billing;

public static class BillingBootstrap
{
    public static void AddBilling<TDbContext>(this WebApplicationBuilder builder)
        where TDbContext : ConsoleDbContext
    {
        builder.Services.AddOptions<StripeOptions>()
            .BindConfiguration("Stripe");

        StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];

        builder.Services.AddScoped<ISharedBillingService, SharedBillingService<TDbContext>>();

        builder.Services.AddHostedService<UserCountUpdaterBackgroundService>();
        builder.Services.AddHostedService<MeteredBillingBackgroundService>();

    }
}