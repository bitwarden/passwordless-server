using System.Text.Json;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.Models;

public class AccountMetaInformation : PerTenant
{
    // Should probably be removed
    public string? SubscriptionTier { get; set; }
    public required string[] AdminEmails { get; set; }

    public string AdminEmailsSerialized
    {
        get
        {
            return JsonSerializer.Serialize(AdminEmails);
        }
        set
        {
            AdminEmails = JsonSerializer.Deserialize<string[]>(value);
        }
    }

    // Should probably be removed
    public required string AcountName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeleteAt { get; set; }

    public AppFeature? Features { get; set; }
    public virtual IReadOnlyCollection<ApplicationEvent>? Events { get; set; }
    public virtual IReadOnlyCollection<PeriodicCredentialReport>? PeriodicCredentialReports { get; set; }
    public virtual IReadOnlyCollection<DispatchedEmail>? DispatchedEmails { get; set; }
}