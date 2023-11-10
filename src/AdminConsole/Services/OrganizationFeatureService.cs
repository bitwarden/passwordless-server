using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class OrganizationFeatureService<TDbContext> : IOrganizationFeatureService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly StripeOptions _options;

    public OrganizationFeatureService(IDbContextFactory<TDbContext> dbContextFactory, IOptions<StripeOptions> options)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
    }

    public FeaturesContext GetOrganizationFeatures(int orgId)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var billingPlans = db.Applications
            .Where(x => x.OrganizationId == orgId)
            .GroupBy(x => x.BillingPlan)
            .Select(x => x.Key)
            .ToList();

        // we cannot reliably set organization features, as there is no such concept.
        FeaturesOptions features;
        if (billingPlans.Contains(PlanConstants.Enterprise))
        {
            features = _options.Plans[PlanConstants.Enterprise].Features;
        }
        else if (billingPlans.Contains(PlanConstants.Pro))
        {
            features = _options.Plans[PlanConstants.Pro].Features;
        }
        else
        {
            features = _options.Plans[PlanConstants.Free].Features;
        }
        return new FeaturesContext(
            features.EventLoggingIsEnabled,
            features.EventLoggingRetentionPeriod,
            null);

    }
}