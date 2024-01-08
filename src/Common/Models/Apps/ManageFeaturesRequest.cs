using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public sealed class ManageFeaturesRequest
{
    public bool EventLoggingIsEnabled { get; init; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; init; }

    [Range(0, 90)]
    public int EventLoggingRetentionPeriod { get; init; }
    
    public bool AllowAttestation { get; init; }
}