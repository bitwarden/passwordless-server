namespace Passwordless.Service.Features;

public sealed class NullFeaturesContext : IFeaturesContext
{
    public bool EventLoggingIsEnabled { get; init; }
    public int EventLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public bool IsInFeaturesContext => false;

}