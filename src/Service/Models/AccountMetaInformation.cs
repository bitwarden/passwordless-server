using System.Text.Json;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.Models;

public class AccountMetaInformation : PerTenant
{
    public required string[] AdminEmails { get; set; }

    public string AdminEmailsSerialized
    {
        get
        {
            return JsonSerializer.Serialize(AdminEmails);
        }
        set
        {
            AdminEmails = ParseAdminEmails(value);
        }
    }

    // Should probably be removed
    public required string AcountName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeleteAt { get; set; }

    public AppFeature? Features { get; set; }
    public virtual IReadOnlyCollection<ApplicationEvent>? Events { get; set; }
    public virtual IReadOnlyCollection<DispatchedEmail>? DispatchedEmails { get; set; }
    public virtual IReadOnlyCollection<PeriodicCredentialReport>? PeriodicCredentialReports { get; set; }
    public virtual IReadOnlyCollection<PeriodicActiveUserReport>? PeriodicActiveUserReports { get; set; }

    private static string[] ParseAdminEmails(string adminEmails)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(adminEmails) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}