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
    /// 
    /// </summary>
    /// <param name="subscriptionItemId"></param>
    /// <param name="users"></param>
    /// <returns></returns>

    Task OnPaidSubscriptionChangedAsync(string customerId, string clientReferenceId, string subscriptionId);

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
}