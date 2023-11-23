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

        // TODO: Introduce an interface and replace it with Noop.
        builder.Services.AddScoped<BillingHelper>();
        // Todo: Improve this self-hosting story.
        if (builder.Configuration.IsSelfHosted())
        {
            builder.Services.AddScoped<ISharedBillingService, NoOpBillingService>();
        }
        else
        {
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];
            builder.Services.AddScoped<ISharedBillingService, SharedBillingService<TDbContext>>();
            builder.Services.AddHostedService<UserCountUpdaterBackgroundService>();
            builder.Services.AddHostedService<MeteredBillingBackgroundService>();
        }

    }
}

public class NoOpBillingService : ISharedBillingService
{
    public Task UpdateUsageAsync()
    {
        throw new NotImplementedException();
    }

    public Task OnSubscriptionCreatedAsync(string customerId, string clientReferenceId, string subscriptionId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSubscriptionStatusAsync(Invoice? dataObject)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetCustomerIdAsync(int organizationId)
    {
        throw new NotImplementedException();
    }

    public Task OnSubscriptionDeletedAsync(string subscriptionId)
    {
        throw new NotImplementedException();
    }

    public Task OnPostApplicationDeletedAsync(string subscriptionItemId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateApplicationAsync(string applicationId, string plan, string subscriptionItemId, string priceId)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateCheckoutSessionAsync(int organizationId, string? billingCustomerId, string email, string planName,
        string successUrl, string cancelUrl)
    {
        throw new NotImplementedException();
    }
}