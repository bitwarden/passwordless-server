using AdminConsole.Services;

namespace Passwordless.AdminConsole.Features;

public class FeaturesContext : IFeaturesContext
{
    private readonly DataService _dataService;

    public FeaturesContext(DataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<bool> IsAuditLoggingEnabled() =>
       (await _dataService.GetOrganization()).BillingSubscriptionId?.ToLower() == "enterprise";
}