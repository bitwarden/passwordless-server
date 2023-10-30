using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Passwordless.Common.Extensions;

namespace Passwordless.Api.HealthChecks;

public class VersionHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return Task.FromResult(
            HealthCheckResult.Healthy("version", new Dictionary<string, object>
            {
                ["version"] = assembly.GetInformationalVersion() ??
                              assembly.GetName().Version?.ToString() ??
                              "unknown version"
            })
        );
    }
}