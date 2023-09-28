using AdminConsole.Db;
using AdminConsole.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Passwordless.AdminConsole.Services;

public class OrganizationFeatureService
{
    private readonly ConsoleDbContext _dbContext;
    private readonly IOptions<PlansOptions> _optionsValue;

    public OrganizationFeatureService(ConsoleDbContext dbContext, IOptions<PlansOptions> optionsValue)
    {
        _dbContext = dbContext;
        _optionsValue = optionsValue;
    }

    public FeaturesContext GetOrganizationFeatures(int orgId)
    {
        if (_dbContext.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "enterprise")
            && _optionsValue.Value.TryGetValue("Enterprise", out var enterprisePlan))
        {
            return new FeaturesContext(enterprisePlan.AuditLoggingIsEnabled, enterprisePlan.AuditLoggingRetentionPeriod, null);
        }

        if (_dbContext.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "pro")
            && _optionsValue.Value.TryGetValue("Pro", out var proPlan))
        {
            return new FeaturesContext(proPlan.AuditLoggingIsEnabled, proPlan.AuditLoggingRetentionPeriod, null);
        }

        if (_dbContext.Applications.AsNoTracking().Any(x => x.OrganizationId == orgId && x.BillingPlan.ToLower() == "free")
            && _optionsValue.Value.TryGetValue("Free", out var freePlan))
        {
            return new FeaturesContext(freePlan.AuditLoggingIsEnabled, freePlan.AuditLoggingRetentionPeriod, null);
        }

        return new FeaturesContext(false, 0, null);
    }
}