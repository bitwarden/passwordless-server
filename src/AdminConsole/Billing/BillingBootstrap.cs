using Microsoft.EntityFrameworkCore;
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
        builder.Services.AddScoped<IBillingHelper, NoopBillingHelper<TDbContext>>();
        
        // Todo: Improve this self-hosting story.
        if (builder.Configuration.IsSelfHosted())
        {
            builder.Services.AddScoped<ISharedBillingService, NoOpBillingService<TDbContext>>();
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

public class NoOpBillingService<TDbContext> :  ISharedBillingService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    public NoOpBillingService(
        IDbContextFactory<TDbContext> dbContextFactory
        )
    {
        _dbContextFactory = dbContextFactory;
    }
    
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

    public async Task UpdateApplicationAsync(string applicationId, string plan, string subscriptionItemId, string priceId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Applications
            .Where(x => x.Id == applicationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.BillingPlan, plan)
                .SetProperty(p => p.BillingSubscriptionItemId, subscriptionItemId)
                .SetProperty(p => p.BillingPriceId, priceId));
    }

    public Task<string> CreateCheckoutSessionAsync(int organizationId, string? billingCustomerId, string email, string planName,
        string successUrl, string cancelUrl)
    {
        throw new NotImplementedException();
    }
}