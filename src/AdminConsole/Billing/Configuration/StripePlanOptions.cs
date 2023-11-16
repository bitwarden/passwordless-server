namespace Passwordless.AdminConsole.Billing.Configuration;

public class StripePlanOptions
{
    public string? PriceId { get; set; }
    public string Sku { get; set; }
    public UiOptions Ui { get; set; }

    public FeaturesOptions Features { get; set; }
}