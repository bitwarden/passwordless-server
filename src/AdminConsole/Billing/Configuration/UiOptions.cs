namespace Passwordless.AdminConsole.Billing.Configuration;

public class UiOptions
{
    public string Price { get; set; }

    public string? PriceHint { get; set; }

    public ICollection<string> Features { get; set; }

    public string Label { get; set; }
}