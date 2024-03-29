using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Services;
using Stripe;
using Stripe.Checkout;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.AdminConsole.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/billing/webhook", ReceiveWebhookAsync);
    }

    public static async Task<IResult> ReceiveWebhookAsync(
        [FromServices] ISharedBillingService billingService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IOptionsSnapshot<BillingOptions> billingOptions,
        [FromServices] ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Stripe");
        var request = httpContextAccessor.HttpContext!.Request;
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                request.Headers["Stripe-Signature"],
                billingOptions.Value.WebhookSecret
            );
            logger.LogInformation($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process webhook");
            return BadRequest();
        }


        switch (stripeEvent.Type)
        {
            case Events.CheckoutSessionCompleted:
                if (stripeEvent.Data.Object is Session session)
                {
                    await billingService.OnSubscriptionCreatedAsync(session.CustomerId, session.ClientReferenceId, session.SubscriptionId);
                }
                break;
            case Events.InvoicePaid:
            case Events.InvoicePaymentFailed:
                if (stripeEvent.Data.Object is Invoice invoice)
                {
                    await billingService.UpdateSubscriptionStatusAsync(invoice);
                }
                break;
            case Events.CustomerSubscriptionDeleted:
                if (stripeEvent.Data.Object is Subscription subscription)
                {
                    await billingService.OnSubscriptionDeletedAsync(subscription.Id);
                }
                break;
            default:
                break;
                // Unhandled event type
        }

        return Ok();
    }
}