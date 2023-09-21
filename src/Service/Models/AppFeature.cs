using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Models;

public class AppFeature : PerTenant
{
    public bool AuditLoggingIsEnabled { get; set; }

    /// <summary>
    /// The audit logging retention period in days.
    /// </summary>
    public int AuditLoggingRetentionPeriod { get; set; }

    /// <summary>
    /// Developer logging is only enabled when an end date has been set, and has to be manually re-enabled every time.
    /// </summary>
    public DateTime? DeveloperLoggingEndsAt { get; set; }

    /// <summary>
    /// The maximum number of users with credentials allowed in the application.
    /// </summary>
    public int? MaxUsers { get; set; }

    public AccountMetaInformation Application { get; set; }
}