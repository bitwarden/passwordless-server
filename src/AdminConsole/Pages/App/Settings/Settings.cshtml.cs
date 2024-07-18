using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SettingsModel : BaseExtendedPageModel
{
    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly IDataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly IApplicationService _appService;
    private readonly ISharedBillingService _billingService;
    private readonly IScopedPasswordlessClient _scopedPasswordlessClient;
    private readonly BillingOptions _billingOptions;

    public SettingsModel(
        ILogger<SettingsModel> logger,
        IDataService dataService,
        ICurrentContext currentContext,
        IHttpContextAccessor httpContextAccessor,
        IApplicationService appService,
        ISharedBillingService billingService,
        IPasswordlessManagementClient managementClient,
        IOptions<BillingOptions> billingOptions,
        IScopedPasswordlessClient scopedPasswordlessClient,
        IEventLogger eventLogger)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _appService = appService;
        _billingService = billingService;
        _scopedPasswordlessClient = scopedPasswordlessClient;
        _billingOptions = billingOptions.Value;
    }

    public Models.Organization Organization { get; set; }

    public string ApplicationId { get; private set; }

    public bool PendingDelete { get; set; }

    public DateTime? DeleteAt { get; set; }

    public Application? Application { get; private set; }

    public bool CanDeleteImmediately { get; private set; }

    public ICollection<PlanModel> Plans { get; } = new List<PlanModel>();

    [BindProperty]
    public bool IsManualTokenGenerationEnabled { get; set; }

    [BindProperty]
    public bool IsMagicLinksEnabled { get; set; }

    private async Task InitializeAsync()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();
        ApplicationId = _currentContext.AppId ?? string.Empty;

        var application = Organization.Applications.FirstOrDefault(x => x.Id == ApplicationId);

        Application = application ?? throw new InvalidOperationException("Application not found.");
        CanDeleteImmediately = await _appService.CanDeleteApplicationImmediatelyAsync(ApplicationId);

        IsManualTokenGenerationEnabled = _currentContext.Features.IsGenerateSignInTokenEndpointEnabled;
        IsMagicLinksEnabled = _currentContext.Features.IsMagicLinksEnabled;
    }

    public async Task OnGet()
    {
        await InitializeAsync();

        if (!Organization.HasSubscription)
        {
            AddPlan(_billingOptions.Store.Free);
        }
        AddPlan(_billingOptions.Store.Pro);
        AddPlan(_billingOptions.Store.Enterprise);

        PendingDelete = Application?.DeleteAt.HasValue ?? false;
        DeleteAt = Application?.DeleteAt;
    }

    /// <summary>
    /// Deletes the application.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userName = User.Identity?.Name ?? Unknown;
        var applicationId = _currentContext.AppId ?? Unknown;

        if (userName == Unknown || applicationId == Unknown)
        {
            _logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", applicationId, userName);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }

        try
        {
            var response = await _appService.MarkDeleteApplicationAsync(applicationId, userName);

            return response.IsDeleted ? RedirectToPage("/Organization/Overview") : RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete application: {appName}.", applicationId);
            return RedirectToPage("/Error", new { ex.Message });
        }
    }

    /// <summary>
    /// Cancels the deletion of the application.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostCancelAsync()
    {
        var applicationId = _currentContext.AppId ?? Unknown;

        if (applicationId == Unknown)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }

        try
        {
            await _appService.CancelDeletionForApplicationAsync(applicationId);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return RedirectToPage("/Error", new { ex.Message });
        }
    }

    /// <summary>
    /// Handles the plan change.
    /// </summary>
    /// <param name="selectedPlan"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostChangePlanAsync(string selectedPlan)
    {
        var appId = _currentContext.AppId!;
        var redirectUrl = await _billingService.ChangePlanAsync(appId, selectedPlan);
        return Redirect(redirectUrl!);
    }

    private void AddPlan(string plan)
    {
        var options = _billingOptions.Plans[plan];
        var isActive = Application!.BillingPlan == plan;
        var isOutdated = isActive && Application!.BillingPriceId != options.PriceId;

        bool canSubscribe;
        if (plan == _billingOptions.Store.Free || Application.DeleteAt.HasValue)
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

    public bool IsAttestationAllowed => _currentContext.Features.AllowAttestation;

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