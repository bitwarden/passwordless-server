namespace Passwordless.Service.Features;

public sealed class NullFeaturesContext : IFeaturesContext
{
    public bool EventLoggingIsEnabled { get; init; }
    public int EventLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public long? MaxUsers { get; init; }
    public bool SignInTokenEndpointEnabled { get; init; } = true;
    public bool IsInFeaturesContext => false;
}