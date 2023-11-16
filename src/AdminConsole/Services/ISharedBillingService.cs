using Stripe;

namespace Passwordless.AdminConsole.Services;

public interface ISharedBillingService
{
    /// <summary>
    /// Updates the seats used for every app in the organization which are relevant for metered subscriptions.
    /// </summary>
    /// <returns></returns>
    Task UpdateUsageAsync();

    /// <summary>
    /// When a customer subscribes for the first time.
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="clientReferenceId"></param>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task OnSubscriptionCreatedAsync(string customerId, string clientReferenceId, string subscriptionId);

    Task UpdateSubscriptionStatusAsync(Invoice? dataObject);

    /// <summary>
    /// Cancels a Stripe subscription.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns>Whether operation was successful</returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> CancelSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Gets the Stripe CustomerId for the organization.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    Task<string?> GetCustomerIdAsync(int organizationId);

    /// <summary>
    /// Executed when a subscription is deleted in Stripe.
    /// 1. Retain the Stripe customer id on the Organization.
    /// 2. Set the plan to Free and reset all other billing/subscription related fields.
    /// </summary>
    /// <param name="subscriptionId">Stripe subscription id</param>
    Task OnSubscriptionDeletedAsync(string subscriptionId);

    /// <summary>
    /// Triggered after an application was deleted.
    /// Responsible for:
    /// - Deleting the subscription item in Stripe if it's not used by any other application inside this organization.
    /// </summary>
    /// <param name="subscriptionItemId"></param>
    Task OnPostApplicationDeletedAsync(string subscriptionItemId);

    Task UpdateApplicationAsync(string applicationId, string plan, string planSku, string subscriptionItemId, string priceId);

    Task<string> CreateCheckoutSessionAsync(
        int organizationId,
        string? billingCustomerId,
        string email,
        string planName,
        string successUrl,
        string cancelUrl);
}