namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public class NewAppOptions
{
    public string AdminEmail { get; set; } = "";
    public bool AuditLoggingIsEnabled { get; set; } = false;
    public int AuditLoggingRetentionPeriod { get; set; } = 365;
}