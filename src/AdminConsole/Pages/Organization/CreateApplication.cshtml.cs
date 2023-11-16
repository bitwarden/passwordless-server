using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Application = Passwordless.AdminConsole.Models.Application;
using NewAppOptions = Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts.NewAppOptions;
using NewAppResponse = Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts.NewAppResponse;

namespace Passwordless.AdminConsole.Pages.Organization;

public class CreateApplicationModel : PageModel
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IOptionsSnapshot<PasswordlessOptions> _passwordlessOptions;
    private readonly IApplicationService _applicationService;
    private readonly IDataService _dataService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly StripeOptions _stripeOptions;

    public CreateApplicationModel(
        IOptionsSnapshot<PasswordlessOptions> passwordlessOptions,
        SignInManager<ConsoleAdmin> signInManager,
        IApplicationService applicationService,
        IDataService dataService,
        IPasswordlessManagementClient managementClient,
        IOptionsSnapshot<StripeOptions> stripeOptions)
    {
        _dataService = dataService;
        _applicationService = applicationService;
        _managementClient = managementClient;
        _passwordlessOptions = passwordlessOptions;
        _signInManager = signInManager;
        _stripeOptions = stripeOptions.Value;
        Form = new CreateApplicationForm
        {
            Plan = _stripeOptions.OnSale.Free
        };
    }

    public CreateApplicationForm Form { get; }

    public Models.Organization Organization { get; set; }

    public ICollection<AvailablePlan> AvailablePlans { get; private set; } = new List<AvailablePlan>();

    public async Task<IActionResult> OnGet()
    {
        CanCreateApplication = await _dataService.AllowedToCreateApplicationAsync();
        await InitializeAsync();

        if (Organization.Applications.Count >= Organization.MaxApplications)
        {
            return RedirectToPage("/billing/manage");
        }

        return Page();
    }

    public bool CanCreateApplication { get; set; }

    public async Task<IActionResult> OnPost(CreateApplicationForm form)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Application app = new()
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            CreatedAt = DateTime.UtcNow,
            OrganizationId = User.GetOrgId().Value
        };

        string email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        // validate we're allowed to create more Orgs
        if (!await _dataService.AllowedToCreateApplicationAsync())
        {
            ModelState.AddModelError("MaxApplications", "You have reached the maximum number of applications for your organization. Please upgrade to a paid organization");
            return Page();
        }

        // Attach a plan
        await InitializeAsync();

        app.BillingPlan = form.Plan;

        if (form.Plan != _stripeOptions.OnSale.Free)
        {
            if (Organization.BillingSubscriptionId == null)
            {
                throw new InvalidOperationException("Cannot create a paid application without a subscription");
            }
            var subscriptionItemService = new SubscriptionItemService();
            var listOptions = new SubscriptionItemListOptions { Subscription = Organization.BillingSubscriptionId };
            var subscriptionItems = await subscriptionItemService.ListAsync(listOptions);

            var subscriptionItem = subscriptionItems.SingleOrDefault(x => x.Price.Id == _stripeOptions.Plans[form.Plan].PriceId);
            if (subscriptionItem == null)
            {
                var createOptions = new SubscriptionItemCreateOptions
                {
                    Subscription = Organization.BillingSubscriptionId,
                    Price = _stripeOptions.Plans[form.Plan].PriceId,
                    ProrationDate = DateTime.UtcNow,
                    ProrationBehavior = "create_prorations"
                };
                subscriptionItem = await subscriptionItemService.CreateAsync(createOptions);
            }

            app.BillingSubscriptionItemId = subscriptionItem.Id;
            app.BillingPriceId = subscriptionItem.Price.Id;
        }

        NewAppResponse res;
        try
        {
            var newAppOptions = new NewAppOptions
            {
                AdminEmail = email,
                EventLoggingIsEnabled = _stripeOptions.Plans[form.Plan].Features.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = _stripeOptions.Plans[form.Plan].Features.EventLoggingRetentionPeriod
            };
            res = await _managementClient.CreateApplication(app.Id, newAppOptions);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("api.failure", "Failed to create your application");
            return Page();
        }

        if (string.IsNullOrEmpty(res.ApiKey1))
        {
            ModelState.AddModelError("ApiCall", res.Message);
            return Page();
        }

        // TODO: Get "Admin Console Keys" and "Real Keys"
        app.ApiKey = res.ApiKey2;
        app.ApiSecret = res.ApiSecret2;
        app.ApiUrl = _passwordlessOptions.Value.ApiUrl;
        app.Onboarding = new Onboarding
        {
            ApiKey = res.ApiKey1,
            ApiSecret = res.ApiSecret1,
            SensitiveInfoExpireAt = DateTime.UtcNow.AddDays(7)
        };

        await _applicationService.CreateApplicationAsync(app);

        var myUser = await _signInManager.UserManager.GetUserAsync(User);
        await _signInManager.RefreshSignInAsync(myUser);

        // TODO: Pass parameters in a better way
        return RedirectToPage("/App/Onboarding/GetStarted", new { app = app.Id });
    }

    private async Task InitializeAsync()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();

        if (Organization.HasSubscription)
        {
            AvailablePlans.Add(new AvailablePlan(_stripeOptions.OnSale.Pro, _stripeOptions.Plans[_stripeOptions.OnSale.Pro].Ui.Label));
            AvailablePlans.Add(new AvailablePlan(_stripeOptions.OnSale.Enterprise, _stripeOptions.Plans[_stripeOptions.OnSale.Enterprise].Ui.Label));
        }
    }

    public class CreateApplicationForm
    {
        [Required, MaxLength(60)]
        public string Name { get; set; }

        [Required, MaxLength(62)]
        public string Id { get; set; }

        [Required, MaxLength(120)]
        public string Description { get; set; }

        [Required]
        public string Plan { get; set; }
    }

    public record AvailablePlan(string Id, string Label);
}