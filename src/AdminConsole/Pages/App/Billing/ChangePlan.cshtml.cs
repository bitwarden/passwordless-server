using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.App.Billing;

public class ChangePlanModel : PageModel
{
    private readonly IDataService _dataService;
    private readonly ISharedBillingService _billingService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly StripeOptions _stripeOptions;

    public ChangePlanModel(
        IDataService dataService,
        ISharedBillingService billingService,
        IPasswordlessManagementClient managementClient,
        IOptions<StripeOptions> stripeOptions)
    {
        _dataService = dataService;
        _billingService = billingService;
        _managementClient = managementClient;
        _stripeOptions = stripeOptions.Value;
    }

    public Application? Application { get; private set; }

    public ICollection<PlanModel> Plans { get; } = new List<PlanModel>();

    public async Task OnGet(string app)
    {
        var organization = await _dataService.GetOrganizationAsync();
        if (!organization.HasSubscription) throw new InvalidOperationException("Organization does not have a subscription.");

        var application = await _dataService.GetApplicationAsync(app);
        if (application == null) throw new InvalidOperationException("Application not found.");
        Application = application;

        AddPlan(PlanConstants.Pro, _stripeOptions.Plans[PlanConstants.Pro]);
        AddPlan(PlanConstants.Enterprise, _stripeOptions.Plans[PlanConstants.Enterprise]);
    }

    public async Task<IActionResult> OnPost(string app, string value)
    {
        var organization = await _dataService.GetOrganizationWithDataAsync();
        if (!organization.HasSubscription) throw new InvalidOperationException("Organization does not have a subscription.");

        var application = organization.Applications.SingleOrDefault(x => x.Id == app);
        var existingSubscriptionItemId = application.BillingSubscriptionItemId;

        var subscriptionItemService = new SubscriptionItemService();

        // Delete subscription item if it's not used by any other application inside this organization.
        if (!organization.Applications.Any(x => x.Id != app && x.BillingSubscriptionItemId == existingSubscriptionItemId))
        {
            await subscriptionItemService.DeleteAsync(existingSubscriptionItemId);
        }

        var plan = _stripeOptions.Plans[value];
        var priceId = plan.PriceId!;
        var subscriptionItem = organization.Applications
            .Where(x => x.Id == priceId)
            .GroupBy(x => new
            {
                x.BillingPriceId,
                x.BillingSubscriptionItemId
            })
            .Select(x => new
            {
                PriceId = x.Key.BillingPriceId!,
                Id = x.Key.BillingSubscriptionItemId!
            }).SingleOrDefault();

        // Create subscription item if it doesn't exist.
        if (subscriptionItem == null)
        {
            var createSubscriptionItemOptions = new SubscriptionItemCreateOptions
            {
                Price = priceId,
                ProrationDate = DateTime.UtcNow,
                ProrationBehavior = "create_prorations",
                Subscription = organization.BillingSubscriptionId
            };
            var createSubscriptionItemResult = await subscriptionItemService.CreateAsync(createSubscriptionItemOptions);
            subscriptionItem = new
            {
                PriceId = createSubscriptionItemResult.Price.Id,
                Id = createSubscriptionItemResult.Id
            };
        }

        await _billingService.UpdateApplicationAsync(app, value, subscriptionItem.Id, priceId);

        var updateFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod
        };
        await _managementClient.SetFeaturesAsync(app, updateFeaturesRequest);

        return RedirectToPage("/billing/manage");
    }

    private void AddPlan(string plan, StripePlanOptions options)
    {
        var isActive = Application!.BillingPlan == plan;

        bool canSubscribe = plan != PlanConstants.Free && Application.BillingPriceId != options.PriceId;

        var model = new PlanModel(
            plan,
            options.PriceId,
            options.Ui.Label,
            options.Ui.Price,
            options.Ui.PriceHint,
            options.Ui.Features.ToImmutableList(),
            isActive,
            canSubscribe);
        Plans.Add(model);
    }

    public record PlanModel(
        string Value,
        string? PriceId,
        string Label,
        string Price,
        string? PriceHint,
        IReadOnlyCollection<string> Features,
        bool IsActive,
        bool CanSubscribe);
}