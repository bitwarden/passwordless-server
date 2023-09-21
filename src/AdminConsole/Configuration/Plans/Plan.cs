namespace Passwordless.AdminConsole.Plans;

public class Plan
{
    public bool AuditLoggingIsEnabled { get; set; }
    public int AuditLoggingRetentionPeriod { get; set; }
    public int? MaxUsers { get; set; }
}