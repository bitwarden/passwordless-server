using System.Text.Json;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.Models;

public class AccountMetaInformation : PerTenant
{
    public string? SubscriptionTier { get; set; }
    public string[]? AdminEmails { get; set; }

    public string AdminEmailsSerialized
    {
        get
        {
            return JsonSerializer.Serialize(AdminEmails);
        }
        set
        {
            var emails = JsonSerializer.Deserialize<string[]>(value);
            if (emails is null)
                return;

            AdminEmails = emails;
        }
    }
    public required string AccountName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public AppFeature? Features { get; set; }
    public virtual IReadOnlyCollection<ApplicationEvent>? Events { get; set; }
}