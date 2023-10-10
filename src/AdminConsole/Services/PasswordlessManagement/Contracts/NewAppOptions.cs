namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public class NewAppOptions
{
    public string AdminEmail { get; set; } = "";
    public bool EventLoggingIsEnabled { get; set; } = false;
    public int EventLoggingRetentionPeriod { get; set; } = 365;
}