namespace Passwordless.Api.Models;

public sealed class SetApplicationFeaturesRequest
{
    public IEnumerable<string> AppIds { get; set; }

    public bool AuditLoggingIsEnabled { get; set; }

    public int AuditLoggingRetentionPeriod { get; set; }
}