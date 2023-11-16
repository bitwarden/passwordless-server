using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
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

        FeaturesOptions features;
        if (!billingPlans.Any())
        {
            features = _options.Plans[_options.OnSale.First()].Features;
        }
        else
        {
            var plan = _options.Plans
                .Where(x => billingPlans.Contains(x.Key))
                .OrderByDescending(x => x.Value.Order)
                .FirstOrDefault();
            features = plan.Value.Features;
        }

        return new FeaturesContext(
            features.EventLoggingIsEnabled,
            features.EventLoggingRetentionPeriod,
            null);
    }
}