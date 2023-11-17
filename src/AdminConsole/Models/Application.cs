namespace Passwordless.AdminConsole.Models;

public class Application
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string ApiUrl { get; set; }

    public virtual Onboarding? Onboarding { get; set; }
    public string BillingPlan { get; set; }
    public string? BillingSubscriptionItemId { get; set; }
    public string? BillingPriceId { get; set; }

    public int CurrentUserCount { get; set; }

    public DateTime? DeleteAt { get; set; }
}