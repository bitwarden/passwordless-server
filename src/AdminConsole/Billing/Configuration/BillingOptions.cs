namespace Passwordless.AdminConsole.Billing.Configuration;

public class BillingOptions
{
    public string WebhookSecret { get; set; }

    public string ApiKey { get; set; }

    public StoreOptions Store { get; set; }

    public Dictionary<string, BillingPlanOptions> Plans { get; set; }
}