using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services;
using Stripe;

namespace Passwordless.AdminConsole.Billing;

public static class AddBillingExtensionMethods
{
    public static void AddBilling<TDbContext>(this WebApplicationBuilder builder)
        where TDbContext : ConsoleDbContext
    {
        builder.Services.AddOptions<StripeOptions>()
            .BindConfiguration("Stripe");

        StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];

        builder.Services.AddScoped<ISharedBillingService, SharedBillingService<TDbContext>>();
    }
}