using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Middleware;
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
            var db = scope.ServiceProvider.GetRequiredService<ConsoleDbContext>();
            var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

            // 1. Retrieve all paid organizations.
            var sharedBillingService = scope.ServiceProvider.GetRequiredService<ISharedBillingService>();
            var organizations = await sharedBillingService.GetPayingOrganizationsAsync();

            // 2. Populate how many unique users per application we have.
            Logger.LogInformation("Updating metered billing for {Organizations} organizations.", organizations.Count);
            foreach (var organization in organizations)
            {
                var items = new Dictionary<string, int>();

                foreach (var application in organization.Applications)
                {
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
        catch (Exception e)
        {
            logger.LogError("Failed to update billing: {error}", e.Message);
        }
    }
}