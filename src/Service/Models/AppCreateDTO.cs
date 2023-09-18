namespace Passwordless.Service.Models;

public record AppCreateDTO
{
    public string AdminEmail { get; set; } = "";
    public bool AuditLoggingIsEnabled { get; set; } = false;
    public int AuditLoggingRetentionPeriod { get; set; } = 365;
    public int? MaxUsers { get; set; }
}