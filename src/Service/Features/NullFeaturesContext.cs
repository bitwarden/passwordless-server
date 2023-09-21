namespace Passwordless.Service.Features;

public sealed class NullFeaturesContext : IFeaturesContext
{
    public bool AuditLoggingIsEnabled { get; init; }
    public int AuditLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public bool IsInFeaturesContext => false;

}