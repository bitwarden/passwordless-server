using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;

namespace Passwordless.AdminConsole.Billing;

public class NoOpBillingService : BaseBillingService, ISharedBillingService
{
    public NoOpBillingService(
        ConsoleDbContext db,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedStripeBillingService> logger,
        IOptions<BillingOptions> billingOptions,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory
    ) : base(db, dataService, passwordlessClient, logger, billingOptions, httpContextAccessor, urlHelperFactory)
    {
    }

    public Task UpdateUsageAsync()
    {
        // This can be a no-op.
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PaymentMethodModel>> GetPaymentMethods(string? organizationBillingCustomerId)
    {
        return Task.FromResult<IReadOnlyCollection<PaymentMethodModel>>(Array.Empty<PaymentMethodModel>().ToImmutableList());
    }

    public Task OnSubscriptionCreatedAsync(string customerId, string clientReferenceId, string subscriptionId)
    {
        // only used in webhook
        throw new NotImplementedException();
    }

    public Task UpdateSubscriptionStatusAsync(Invoice? dataObject)
    {
        // Only used in webhook
        throw new NotImplementedException();
    }

    public Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        // Deleting org
        // noop
        return Task.FromResult(true);
    }

    public Task<string?> GetCustomerIdAsync(int organizationId)
    {
        // can be noop, only used to open stripe to manage billing
        throw new NotImplementedException();
    }

    public Task OnSubscriptionDeletedAsync(string subscriptionId)
    {
        // only used in webhook
        throw new NotImplementedException();
    }

    public Task OnPostApplicationDeletedAsync(string subscriptionItemId)
    {
        // can be noop
        return Task.CompletedTask;
    }

    public async Task<string?> GetRedirectToUpgradeOrganization(string selectedPlan)
    {
        // Upgrade org
        var organization = await _dataService.GetOrganizationWithDataAsync();

        // Todo: this should be called something better
        await UpgradeToPaidOrganization("simple", selectedPlan, organization.Id, "simple", DateTime.UtcNow, "simple", "simple");

        // I don't link to return these strings
        return "/billing/manage";
    }

    public async Task<string?> ChangePlanAsync(string app, string selectedPlan)
    {
        await this.SetPlanOnApp(app, selectedPlan, "simple", "simple");

        // TODO: returning this string is a bit werid
        // It's also sending us to the wrong place!
        return "/billing/manage";
    }

    public Task<IReadOnlyCollection<InvoiceModel>> GetInvoicesAsync()
    {
        return Task.FromResult<IReadOnlyCollection<InvoiceModel>>(new List<InvoiceModel>(0).ToImmutableList());
    }

    public Task<(string subscriptionItemId, string priceId)> CreateSubscriptionItem(Organization org, string planSKU)
    {
        return Task.FromResult(new ValueTuple<string, string>("simple", "simple"));
    }

    public Task<string> GetManagementUrl(int orgId)
    {
        // can be noop
        return Task.FromResult("/");
    }
}