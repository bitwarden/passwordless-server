namespace Passwordless.AdminConsole.Models.DTOs;

public class AppFeatureDto
{
    public bool AuditLoggingIsEnabled { get; set; }
    public int AuditLoggingRetentionPeriod { get; set; }
    public DateTime? DeveloperLoggingEndsAt { get; set; }
}