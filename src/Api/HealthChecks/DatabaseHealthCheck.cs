using Microsoft.Extensions.Diagnostics.HealthChecks;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ITenantStorage _tenantStorage;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ITenantStorage tenantStorage, ILogger<DatabaseHealthCheck> logger)
    {
        _tenantStorage = tenantStorage;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tenantStorage.GetAccountInformation();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }
}