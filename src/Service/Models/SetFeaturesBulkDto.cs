namespace Passwordless.Service.Models;

public sealed class SetFeaturesBulkDto : SetFeaturesDto
{
    public IEnumerable<string> Tenants { get; set; }
    public bool AuditLoggingIsEnabled { get; set; }
}