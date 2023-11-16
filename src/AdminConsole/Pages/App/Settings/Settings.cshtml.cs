using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Stripe.Checkout;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SettingsModel : PageModel
{
    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly IDataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly IApplicationService _appService;
    private readonly ISharedBillingService _billingService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly StripeOptions _stripeOptions;

    public SettingsModel(
        ILogger<SettingsModel> logger,
        IDataService dataService,
        ICurrentContext currentContext,
        IApplicationService appService,
        ISharedBillingService billingService,
        IPasswordlessManagementClient managementClient,
        IOptions<StripeOptions> stripeOptions)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _appService = appService;
        _billingService = billingService;
        _managementClient = managementClient;
        _stripeOptions = stripeOptions.Value;
    }

    public Models.Organization Organization { get; set; }

    public string ApplicationId { get; set; }

    public bool PendingDelete { get; set; }

    public DateTime? DeleteAt { get; set; }

    public Application? Application { get; private set; }

    public ICollection<PlanModel> Plans { get; } = new List<PlanModel>();

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();
        ApplicationId = _currentContext.AppId ?? String.Empty;

        var application = Organization.Applications.FirstOrDefault(x => x.Id == ApplicationId);

        if (application == null) throw new InvalidOperationException("Application not found.");
        Application = application;

        if (!Organization.HasSubscription)
        {
            AddPlan(_stripeOptions.OnSale.Free);
        }
        AddPlan(_stripeOptions.OnSale.Pro);
        AddPlan(_stripeOptions.OnSale.Enterprise);

        PendingDelete = application?.DeleteAt.HasValue ?? false;
        DeleteAt = application?.DeleteAt;
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userName = User.Identity?.Name ?? Unknown;
        var applicationId = _currentContext.AppId ?? Unknown;

        if (userName == Unknown || applicationId == Unknown)
        {
            _logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", applicationId, userName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected happened. Please try again later." });
        }

        var response = await _appService.MarkApplicationForDeletionAsync(applicationId, userName);

        return response.IsDeleted ? RedirectToPage("/Organization/Overview") : RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        var applicationId = _currentContext.AppId ?? Unknown;

        try
        {
            await _appService.CancelDeletionForApplicationAsync(applicationId);

            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public async Task<IActionResult> OnPostChangePlanAsync(string app, string selectedPlan)
    {
        var organization = await _dataService.GetOrganizationWithDataAsync();
        if (!organization.HasSubscription)
        {
            var successUrl = Url.PageLink("/Billing/Success");
            successUrl += "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.PageLink("/Billing/Cancelled");
            var sessionUrl = await _billingService.CreateCheckoutSessionAsync(organization.Id, organization.BillingCustomerId, User.GetEmail(), selectedPlan, successUrl, cancelUrl);
            return Redirect(sessionUrl);
        }

        var application = organization.Applications.SingleOrDefault(x => x.Id == app);
        var existingSubscriptionItemId = application.BillingSubscriptionItemId;

        var plan = _stripeOptions.Plans[selectedPlan];
        var priceId = plan.PriceId!;
        var subscriptionItem = organization.Applications
            .Where(x => x.BillingPriceId == priceId)
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

        var subscriptionItemService = new SubscriptionItemService();

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

        // Delete subscription item if it's not used by any other application inside this organization.
        if (!organization.Applications.Any(x => x.Id != app && x.BillingSubscriptionItemId == existingSubscriptionItemId))
        {
            var deleteSubscriptionItemOptions = new SubscriptionItemDeleteOptions { ClearUsage = true };
            await subscriptionItemService.DeleteAsync(existingSubscriptionItemId, deleteSubscriptionItemOptions);
        }

        await _billingService.UpdateApplicationAsync(app, selectedPlan, subscriptionItem.Id, priceId);

        var updateFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod
        };
        await _managementClient.SetFeaturesAsync(app, updateFeaturesRequest);

        return RedirectToPage("/billing/manage");
    }

    /// <summary>
    /// Creates a new checkout session in Stripe's payment portal.
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="billingCustomerId"></param>
    /// <param name="planName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<IActionResult> CreateCheckoutSessionAsync(
        int organizationId,
        string? billingCustomerId,
        string planName)
    {
        if (_stripeOptions.Plans.All(x => x.Key != planName))
        {
            throw new ArgumentException("Invalid plan name");
        }

        var successUrl = Url.PageLink("/Billing/Success");
        successUrl += "?session_id={CHECKOUT_SESSION_ID}";

        var cancelUrl = Url.PageLink("/Billing/Cancelled");
        var options = new SessionCreateOptions
        {
            Metadata =
                new Dictionary<string, string>
                {
                    { "orgId", organizationId.ToString() },
                    { "passwordless", "passwordless" }
                },
            ClientReferenceId = organizationId.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _stripeOptions.Plans[planName].PriceId,
                }
            }
        };

        if (billingCustomerId != null)
        {
            options.Customer = billingCustomerId;
        }
        else
        {
            options.TaxIdCollection = new SessionTaxIdCollectionOptions { Enabled = true, };
            options.CustomerEmail = User.GetEmail();
        }

        var service = new SessionService();
        Session? session = await service.CreateAsync(options);

        return Redirect(session.Url);
    }

    private void AddPlan(string plan)
    {
        var options = _stripeOptions.Plans[plan];
        var isActive = Application!.BillingPlan == plan;
        var isOutdated = isActive && Application!.BillingPriceId != options.PriceId;

        bool canSubscribe;
        if (plan == _stripeOptions.OnSale.Free || Application.DeleteAt.HasValue)
        {
            canSubscribe = false;
        }
        else
        {
            canSubscribe = Application.BillingPriceId != options.PriceId;
        }

        var model = new PlanModel(
            plan,
            options.PriceId,
            options.Ui.Label,
            options.Ui.Price,
            options.Ui.PriceHint,
            options.Ui.Features.ToImmutableList(),
            isActive,
            canSubscribe,
            isOutdated);
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
        bool CanSubscribe,
        bool IsOutdated);
}