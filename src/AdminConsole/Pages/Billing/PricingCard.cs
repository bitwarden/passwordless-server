using System.Globalization;
using Passwordless.AdminConsole.Billing.Configuration;

namespace Passwordless.AdminConsole.Pages.Billing;

public class PricingCardModel
{
    private static readonly CultureInfo PriceFormat = new("en-US");
    
    public PricingCardModel(
        string name,
        StripePlanOptions plan)
    {
        Name = name;
        Plan = plan;
    }
    
    public string Name { get; }
    
    public StripePlanOptions Plan { get; }
    
    public bool IsActive { get; set; }

    public string GetPrice() => $"{Plan.Price.ToString("C", PriceFormat)}";
}