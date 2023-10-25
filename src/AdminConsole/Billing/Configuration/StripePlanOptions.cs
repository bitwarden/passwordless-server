namespace Passwordless.AdminConsole.Billing.Configuration;

public class StripePlanOptions
{
    /// <summary>
    /// The Stripe price id for a product related to a plan.
    /// </summary>
    public string PriceId { get; set; }

    /// <summary>
    /// Price per user per month
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// If you want to display a price hint at the bottom of a price card, for example related to tiered pricing.
    /// </summary>
    public string? PriceHint { get; set; }

    /// <summary>
    /// The features that are included in a plan.
    /// </summary>
    public ICollection<string> Features { get; set; }
}