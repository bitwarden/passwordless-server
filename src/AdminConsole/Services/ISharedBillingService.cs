using Stripe;

namespace Passwordless.AdminConsole.Services;

public interface ISharedBillingService
{
    Task UpdateUsageAsync();

    Task UpdateStripeAsync(string subscriptionItemId, int users);

    Task ConvertFromFreeToPaidAsync(string customerId, string clientReferenceId, string subscriptionId);

    Task UpdateSubscriptionStatusAsync(Invoice? dataObject);

    /// <summary>
    /// Cancels a Stripe subscription.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns>Whether operation was successful</returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
}