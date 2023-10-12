using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class OrganizationFeatureService<TDbContext> : IOrganizationFeatureService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IOptions<PlansOptions> _optionsValue;

    public OrganizationFeatureService(IDbContextFactory<TDbContext> dbContextFactory, IOptions<PlansOptions> optionsValue)
    {
        _dbContextFactory = dbContextFactory;
        _optionsValue = optionsValue;
    }

    public FeaturesContext GetOrganizationFeatures(int orgId)
    {
        using var db = _dbContextFactory.CreateDbContext();
        if (db.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "enterprise")
            && _optionsValue.Value.TryGetValue("Enterprise", out var enterprisePlan))
        {
            return new FeaturesContext(enterprisePlan.EventLoggingIsEnabled, enterprisePlan.EventLoggingRetentionPeriod, null);
        }

        if (db.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "pro")
            && _optionsValue.Value.TryGetValue("Pro", out var proPlan))
        {
            return new FeaturesContext(proPlan.EventLoggingIsEnabled, proPlan.EventLoggingRetentionPeriod, null);
        }

        if (db.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "free")
            && _optionsValue.Value.TryGetValue("Free", out var freePlan))
        {
            return new FeaturesContext(freePlan.EventLoggingIsEnabled, freePlan.EventLoggingRetentionPeriod, null);
        }

        return new FeaturesContext(false, 0, null);
    }
}