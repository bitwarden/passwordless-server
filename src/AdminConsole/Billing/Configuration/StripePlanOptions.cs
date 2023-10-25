namespace Passwordless.AdminConsole.Billing.Configuration;

public class StripePlanOptions
{
    public string PriceId { get; set; }

    public decimal Price { get; set; }

    public string? PriceHint { get; set; }

    public ICollection<string> Features { get; set; }
}