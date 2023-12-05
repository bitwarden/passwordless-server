namespace Passwordless.AdminConsole.Models.DTOs;

public sealed class SetApplicationFeaturesRequest
{
    public bool EventLoggingIsEnabled { get; set; }

    public int EventLoggingRetentionPeriod { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }
}