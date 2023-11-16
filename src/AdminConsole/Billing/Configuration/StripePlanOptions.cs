namespace Passwordless.AdminConsole.Billing.Configuration;

public class StripePlanOptions
{
    public ushort Order { get; set; }
    public string? PriceId { get; set; }
    public UiOptions Ui { get; set; }

    public FeaturesOptions Features { get; set; }
}