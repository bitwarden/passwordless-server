namespace Passwordless.Service.Models;

public record AppCreateDTO : AppIdDTO
{
    public string AdminEmail { get; set; } = "";
    public bool AuditLoggingIsEnabled { get; set; } = false;
    public int AuditLoggingRetentionPeriod { get; set; } = 365;
}