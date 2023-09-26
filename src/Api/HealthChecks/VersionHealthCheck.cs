using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Passwordless.Api.HealthChecks;

public class VersionHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return HealthCheckResult.Healthy(version.ToString(), new Dictionary<string, object>
        {
            {nameof(version.Major), version.Major},
            {nameof(version.Minor), version.Minor},
            {nameof(version.Revision), version.Revision},
            {nameof(version.Build), version.Build},
        });
    }
}