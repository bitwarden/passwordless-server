using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Components.Shared.Icons.PaymentMethods;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services;

public record UsageItem(string BillingSubscriptionItemId, int Users);

public class BaseBillingService
{
    protected readonly ConsoleDbContext Db;

    protected readonly IPasswordlessManagementClient _passwordlessClient;
    protected readonly ILogger<BaseBillingService> _logger;
    protected readonly BillingOptions _billingOptions;
    protected readonly IDataService _dataService;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    public BaseBillingService(
        ConsoleDbContext db,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<BaseBillingService> logger,
        IOptions<BillingOptions> billingOptions,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory

        )
    {
        Db = db;
        _dataService = dataService;
        _passwordlessClient = passwordlessClient;
        _logger = logger;
        _billingOptions = billingOptions.Value;
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    protected async Task SetPlanOnApp(string app, string selectedPlan, string subscriptionItemId, string priceId)
    {
        var plan = _billingOptions.Plans[selectedPlan];
        await this.UpdateApplicationAsync(app, selectedPlan, subscriptionItemId, priceId);

        var updateFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod,
            MagicLinkEmailMonthlyQuota = plan.Features.MagicLinkEmailMonthlyQuota,
            MaxUsers = plan.Features.MaxUsers,
            AllowAttestation = plan.Features.AllowAttestation
        };

        await _passwordlessClient.SetFeaturesAsync(app, updateFeaturesRequest);
    }

    protected async Task UpgradeToPaidOrganization(string customerId, string planName, int orgId, string subscriptionId, DateTime subscriptionCreatedAt, string subscriptionItemId, string subscriptionItemPriceId)
    {
        var features = _billingOptions.Plans[planName].Features;

        // SetCustomerId on the Org
        var org = await Db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);

        if (org == null)
        {
            throw new InvalidOperationException("Org not found");
        }

        if (org.HasSubscription)
        {
            return;
        }

        org.BillingCustomerId = customerId;
        org.BecamePaidAt = subscriptionCreatedAt;
        org.BillingSubscriptionId = subscriptionId;

        // Increase the limits
        org.MaxAdmins = features.MaxAdmins;
        org.MaxApplications = features.MaxApplications;

        if (planName == null)
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }


        var applications = await Db.Applications
            .Where(a => a.OrganizationId == orgId)
            .ToListAsync();

        var setFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
            MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota,
            MaxUsers = features.MaxUsers,
            AllowAttestation = features.AllowAttestation
        };

        // set the plans on each app
        foreach (var application in applications)
        {
            application.BillingPlan = planName;
            application.BillingSubscriptionItemId = subscriptionItemId;
            application.BillingPriceId = subscriptionItemPriceId;
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }

        await Db.SaveChangesAsync();
    }

    protected async Task<List<UsageItem>> GetUsageItems()
    {
        var items = await Db.Applications
            .Where(a => a.BillingSubscriptionItemId != null)
            .GroupBy(a => new
            {
                a.OrganizationId,
                a.BillingSubscriptionItemId
            })
            .Select(g => new
                UsageItem(g.Key.BillingSubscriptionItemId, g.Sum(x => x.CurrentUserCount)))
            .ToListAsync();
        return items;
    }

    /// <inheritdoc />
    public async Task<string?> GetCustomerIdAsync(int organizationId)
    {
        var customerId = await Db.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.BillingCustomerId)
            .FirstOrDefaultAsync();
        return customerId;
    }

    public async Task UpdateApplicationAsync(string applicationId, string plan, string subscriptionItemId, string priceId)
    {
        await Db.Applications
            .Where(x => x.Id == applicationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.BillingPlan, plan)
                .SetProperty(p => p.BillingSubscriptionItemId, subscriptionItemId)
                .SetProperty(p => p.BillingPriceId, priceId));
    }

    /// <inheritdoc />
    public async Task OnSubscriptionDeletedAsync(string subscriptionId)
    {
        var organization = await Db.Organizations
            .Include(x => x.Applications)
            .SingleOrDefaultAsync(x => x.BillingSubscriptionId == subscriptionId);

        if (organization == null) return;

        organization.BillingSubscriptionId = null;
        organization.BecamePaidAt = null;

        var features = _billingOptions.Plans[_billingOptions.Store.Free].Features;
        organization.MaxAdmins = features.MaxAdmins;
        organization.MaxApplications = features.MaxApplications;

        foreach (var application in organization.Applications)
        {
            application.BillingPriceId = null;
            application.BillingSubscriptionItemId = null;
            application.BillingPlan = _billingOptions.Store.Free;
        }

        await Db.SaveChangesAsync();

        var setFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
            MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota,
            MaxUsers = features.MaxUsers,
            AllowAttestation = features.AllowAttestation
        };
        foreach (var application in organization.Applications)
        {
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }
    }
}

public record PricingCardModel(
    string Name,
    BillingPlanOptions Plan)
{
    /// <summary>
    /// We want to display the price in US dollars.
    /// </summary>
    private static readonly CultureInfo PriceFormat = new("en-US");

    /// <summary>
    /// Indicates if the plan is the active plan for the organization.
    /// </summary>
    public bool IsActive { get; set; }
}

public record PaymentMethodModel(string Brand, string Number, DateTime ExpirationDate)
{
    public Type CardIcon
    {
        get
        {
            return Brand switch
            {
                "amex" => typeof(AmexIcon),
                "diners" => typeof(DinersIcon),
                "discover" => typeof(DiscoverIcon),
                "jcb" => typeof(JcbIcon),
                "mastercard" => typeof(MasterCardIcon),
                "unionpay" => typeof(UnionPayIcon),
                "visa" => typeof(VisaIcon),
                _ => typeof(UnknownCardIcon)
            };
        }
    }
}