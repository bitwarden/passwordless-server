using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Services;
using Stripe;
using Session = Stripe.Checkout.Session;

namespace Passwordless.AdminConsole.Pages.Billing;

[AllowAnonymous]
[IgnoreAntiforgeryToken(Order = 2000)]
public class Webhook : PageModel
{
    private readonly ISharedBillingService _sharedBillingService;
    private readonly StripeOptions _stripeOptions;

    public Webhook(ISharedBillingService sharedBillingService, IOptions<StripeOptions> stripeOptions)
    {
        _sharedBillingService = sharedBillingService;
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
                    await _sharedBillingService.OnSubscriptionCreatedAsync(session.CustomerId, session.ClientReferenceId, session.SubscriptionId);
                }
                break;
            case Events.InvoicePaid:
            case Events.InvoicePaymentFailed:
                if (stripeEvent.Data.Object is Invoice invoice)
                {
                    await _sharedBillingService.UpdateSubscriptionStatusAsync(invoice);
                }
                break;
            case Events.CustomerSubscriptionDeleted:
                if (stripeEvent.Data.Object is Subscription subscription)
                {
                    await _sharedBillingService.OnSubscriptionDeletedAsync(subscription.Id);
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