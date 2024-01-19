using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

/// <summary>
/// Will work as a "patched" request. Each property is nullable with exception of <ref name="PerformedBy"/>. 
/// </summary>
public record SetFeaturesRequest
{
    /// <summary>
    /// Name of the user who performed the action. Null, empty, or whitespace will default to "Api"
    /// </summary>
    public string PerformedBy { get; init; } = "Api";

    /// <summary>
    /// Gets or sets the retention period for event logging in days.
    /// </summary>
    [Range(0, 90)]
    public int? EventLoggingRetentionPeriod { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether manually generated authentication tokens are enabled.
    /// </summary>
    public bool? EnableManuallyGeneratedAuthenticationTokens { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether magic links are enabled.
    /// </summary>
    public bool? EnableMagicLinks { get; init; }
}