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
        var billingPlan = db.Organizations
            .Where(x => x.Id == orgId)
            .Select(x => x.BillingPlan)
            .First();
        var options = _optionsValue.Value[billingPlan];
        return new FeaturesContext(
            options.EventLoggingIsEnabled,
            options.EventLoggingRetentionPeriod,
            null);

    }
}