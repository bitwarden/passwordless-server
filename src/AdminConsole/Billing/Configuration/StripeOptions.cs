namespace Passwordless.AdminConsole.Billing.Configuration;

public class StripeOptions
{
    public string WebhookSecret { get; set; }

    public string ApiKey { get; set; }

    public Dictionary<string, StripePlanOptions> Plans { get; set; }
}