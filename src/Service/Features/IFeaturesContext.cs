namespace Passwordless.Service.Features;

public interface IFeaturesContext
{
    public bool EventLoggingIsEnabled { get; init; }
    public int EventLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public bool IsInFeaturesContext { get; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    long? MaxUsers { get; init; }
}