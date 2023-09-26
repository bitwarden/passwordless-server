using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Passwordless.Api.HealthChecks;

public class SimpleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}