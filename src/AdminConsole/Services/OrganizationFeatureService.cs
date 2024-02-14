using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class OrganizationFeatureService : IOrganizationFeatureService
{
    private readonly ConsoleDbContext _db;
    private readonly BillingOptions _options;

    public OrganizationFeatureService(ConsoleDbContext db, IOptions<BillingOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public OrganizationFeaturesContext GetOrganizationFeatures(int orgId)
    {
        var billingPlans = _db.Applications
            .Where(x => x.OrganizationId == orgId)
            .GroupBy(x => x.BillingPlan)
            .Select(x => x.Key)
            .ToList();

        FeaturesOptions features;
        if (!billingPlans.Any())
        {
            features = _options.Plans[_options.Store.Free].Features;
        }
        else
        {
            var plan = _options.Plans
                .Where(x => billingPlans.Contains(x.Key))
                .OrderByDescending(x => x.Value.Order)
                .FirstOrDefault();
            features = plan.Value.Features;
        }

        return new OrganizationFeaturesContext(
            features.EventLoggingIsEnabled,
            features.EventLoggingRetentionPeriod);
    }
}