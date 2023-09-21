using AdminConsole.Billing;
using AdminConsole.Db;
using AdminConsole.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Stripe;
using Session = Stripe.Checkout.Session;

namespace AdminConsole.Pages.Billing;

[AllowAnonymous]
[IgnoreAntiforgeryToken(Order = 2000)]
public class Webhook : PageModel
{
    private readonly SharedBillingService _sharedBillingService;
    private readonly ConsoleDbContext _dbContext;
    private readonly StripeOptions _stripeOptions;

    public Webhook(SharedBillingService sharedBillingService, ConsoleDbContext dbContext, IOptions<StripeOptions> stripeOptions)
    {
        _sharedBillingService = sharedBillingService;
        _dbContext = dbContext;
        _stripeOptions = stripeOptions.Value;
    }

    public async Task<IActionResult> OnPost()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeOptions.WebhookSecret
            );
            Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something failed {e}");
            return BadRequest();
        }


        switch (stripeEvent.Type)
        {
            case Events.CheckoutSessionCompleted:
                if (stripeEvent.Data.Object is Session session)
                {
                    await _sharedBillingService.ConvertFromFreeToPaidAsync(session.CustomerId, session.ClientReferenceId, session.SubscriptionId);
                }
                break;
            case "invoice.paid":
            case "invoice.payment_failed":
                if (stripeEvent.Data.Object is Invoice invoice)
                {
                    await _sharedBillingService.UpdateSubscriptionStatus(invoice);
                }
                break;
            default:
                break;
                // Unhandled event type
        }

        return new OkResult();
    }

    public bool HasPasswordlessKey(IHasMetadata obj)
    {
        return obj.Metadata.ContainsKey("passwordless");
    }
}