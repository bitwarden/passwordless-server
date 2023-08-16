namespace Passwordless.Service.Models;

public class SetFeaturesDto
{
    public bool AuditLoggingIsEnabled { get; set; }

    public int AuditLoggingRetentionPeriod { get; set; }
}