namespace Passwordless.Service.Models;

public sealed class ManageFeaturesDto : SetFeaturesDto
{
    public bool AuditLoggingIsEnabled { get; set; }
}