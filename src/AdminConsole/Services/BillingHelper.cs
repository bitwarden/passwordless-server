using System.Collections.Immutable;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Pages.Billing;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;

namespace Passwordless.AdminConsole.Services;


public class NoopBillingHelper<TDbContext> where TDbContext : ConsoleDbContext
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ISharedBillingService _sharedBillingService;
    private readonly IDataService _dataService;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly StripeOptions _stripeOptions;

    public NoopBillingHelper(
        IPasswordlessManagementClient managementClient, IOptions<StripeOptions> stripeOptions, ISharedBillingService sharedBillingService,IDataService dataService, IDbContextFactory<TDbContext> dbContextFactory, IActionContextAccessor actionContextAccessor)
    {
        _managementClient = managementClient;
        _sharedBillingService = sharedBillingService;
        _dataService = dataService;
        _dbContextFactory = dbContextFactory;
        _actionContextAccessor = actionContextAccessor;
        _stripeOptions = stripeOptions.Value;
    }
    
    public async Task<string> ChangePlanASync(string app, string selectedPlan)
    {
        var plan = _stripeOptions.Plans[selectedPlan];

        var updateFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod
        };
        
        await _sharedBillingService.UpdateApplicationAsync(app, selectedPlan, "simple", "simple");
        
        await _managementClient.SetFeaturesAsync(app, updateFeaturesRequest);

        return "/billing/manage";
    }

    public async Task<string> UpgradeOrganization()
    {
        
        var orgId = _actionContextAccessor.ActionContext.HttpContext?.User?.GetOrgId();
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        
        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);

        if (org == null)
        {
            throw new InvalidOperationException("Org not found");
        }

        if (org.HasSubscription)
        {
            return "/";
        }
        org.BillingCustomerId = "simple";
        org.BecamePaidAt = DateTime.Now;
        org.BillingSubscriptionId = "simple";

        var plan = _stripeOptions.Plans[_stripeOptions.Store.Pro];
                
        
        if (plan == null)
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }
        
        var features = plan.Features;

        // Increase the limits
        org.MaxAdmins = features.MaxAdmins;
        org.MaxApplications = features.MaxApplications;



        
        var applications = await db.Applications
            .Where(a => a.OrganizationId == orgId)
            .ToListAsync();


        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };

        // set the plans on each app
        foreach (var application in applications)
        {
            application.BillingPlan = _stripeOptions.Store.Pro;
            application.BillingSubscriptionItemId = "simple";
            application.BillingPriceId = "simple";
            await _managementClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }

        
        await db.SaveChangesAsync();

        return "/";
    }

    public bool IsBillingEnabled { get; set; } = false;
}