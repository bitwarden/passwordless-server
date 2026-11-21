using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;
using Passwordless.Common.Models.Reporting;
using Stripe;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for synchronizing the metered seat usage with our billing partner.
/// </summary>
public sealed class MeteredBillingBackgroundService(
    IServiceProvider services,
    TimeProvider timeProvider,
    ILogger<MeteredBillingBackgroundService> logger)
    : BasePeriodicBackgroundService(new TimeOnly(23, 0, 0), TimeSpan.FromDays(1), timeProvider, logger)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = services.CreateScope();
            var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

            // 1. Retrieve all paid organizations.
            var sharedBillingService = scope.ServiceProvider.GetRequiredService<ISharedBillingService>();
            var organizations = await sharedBillingService.GetPayingOrganizationsAsync();

            Logger.LogInformation("Updating metered billing for {Organizations} organizations.", organizations.Count);
            foreach (var organization in organizations)
            {
                // A failure for one organization must not prevent the remaining organizations from being billed.
                try
                {
                    await UpdateOrganizationUsageAsync(scope, timeProvider, organization);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to update metered billing for organization {OrganizationId}.", organization.Id);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError("Failed to update billing: {error}", e.Message);
        }
    }

    private async Task UpdateOrganizationUsageAsync(IServiceScope scope, TimeProvider timeProvider, Organization organization)
    {
        // 2. Populate how many unique users per application we have.
        var items = new Dictionary<string, int>();

        foreach (var application in organization.Applications)
        {
            // Applications on the free plan have no subscription item, so there is nothing to report.
            if (string.IsNullOrEmpty(application.BillingSubscriptionItemId))
            {
                continue;
            }

            Logger.LogInformation("Processing metered billing for application '{Application}'.", application.Id);

            var context = scope.ServiceProvider.GetRequiredService<ICurrentContext>();
            context.SetApp(application);

            var request = new PeriodicCredentialReportRequest(DateOnly.FromDateTime(timeProvider.GetUtcNow().Date.AddDays(-1)), null);
            var api = scope.ServiceProvider.GetRequiredService<IScopedPasswordlessClient>();
            var reports = await api.GetPeriodicCredentialReportsAsync(request);
            var report = reports.LastOrDefault();

            if (report == null) continue;

            if (items.ContainsKey(application.BillingSubscriptionItemId))
            {
                items[application.BillingSubscriptionItemId] += report.Users;
            }
            else
            {
                items.Add(application.BillingSubscriptionItemId, report.Users);
            }
        }

        // 3. Update the usage records.
        foreach (var item in items)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var service = new UsageRecordService();
            try
            {
                await service.CreateAsync(
                    item.Key,
                    new UsageRecordCreateOptions
                    {
                        Quantity = item.Value,
                        Timestamp = DateTime.UtcNow,
                        Action = "set"
                    },
                    new RequestOptions
                    {
                        IdempotencyKey = idempotencyKey
                    }
                );
            }
            catch (StripeException e)
            {
                Logger.LogError("Usage report failed for item {BillingSubscriptionItemId}:", item.Key);
                Logger.LogError(e, "Idempotency key: {IdempotencyKey}.", idempotencyKey);
            }
        }
    }
}