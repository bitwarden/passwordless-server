namespace Passwordless.AdminConsole.Models.DTOs;

public sealed class SetApplicationFeaturesRequest
{
    public bool AuditLoggingIsEnabled { get; set; }

    public int AuditLoggingRetentionPeriod { get; set; }
}