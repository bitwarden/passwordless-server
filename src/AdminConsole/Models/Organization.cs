using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Models;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrganizationType InfoOrgType { get; set; }
    public UseCaseType InfoUseCase { get; set; }

    public virtual IEnumerable<ConsoleAdmin> Admins { get; set; }

    public List<Application> Applications { get; set; }
    public string? BillingCustomerId { get; set; }
    public string? BillingSubscriptionId { get; set; }
    public DateTime? BecamePaidAt { get; set; }

    public bool HasSubscription => !string.IsNullOrEmpty(BillingSubscriptionId);
    public int MaxApplications { get; set; } = 1;
    public int MaxAdmins { get; set; } = 1;
    public bool IsMagicLinksEnabled { get; set; } = true;
}