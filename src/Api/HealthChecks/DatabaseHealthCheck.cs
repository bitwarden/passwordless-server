using Microsoft.Extensions.Diagnostics.HealthChecks;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ITenantStorageFactory _tenantStorageFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ITenantStorageFactory tenantStorageFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _tenantStorageFactory = tenantStorageFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var storage = _tenantStorageFactory.Create(null);
            await storage.GetAccountInformation();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }
}