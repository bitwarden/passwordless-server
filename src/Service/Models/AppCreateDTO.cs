namespace Passwordless.Service.Models;

public record AppCreateDTO
{
    public string AdminEmail { get; set; } = "";
    public bool EventLoggingIsEnabled { get; set; } = false;
    public int EventLoggingRetentionPeriod { get; set; } = 365;
}