using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Passwordless.Api.HealthChecks;

public class SimpleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}