namespace Passwordless.AdminConsole.Models.DTOs;

public sealed class SetApplicationFeaturesRequest
{
    public bool EventLoggingIsEnabled { get; set; }

    public int EventLoggingRetentionPeriod { get; set; }
}