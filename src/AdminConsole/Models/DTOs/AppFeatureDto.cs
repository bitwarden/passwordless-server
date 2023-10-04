namespace Passwordless.AdminConsole.Models.DTOs;

public class AppFeatureDto
{
    public bool EventLoggingIsEnabled { get; set; }
    public int EventLoggingRetentionPeriod { get; set; }
    public DateTime? DeveloperLoggingEndsAt { get; set; }
}