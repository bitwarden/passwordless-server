namespace AdminConsole.Billing;

public class StripeOptions
{
    public string WebhookSecret { get; set; }
    public string ApiKey { get; set; }
    public string UsersProPriceId { get; set; }
    public string UsersProPlanName { get; set; }
}