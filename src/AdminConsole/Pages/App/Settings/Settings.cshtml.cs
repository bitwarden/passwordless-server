using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SettingsModel : BaseExtendedPageModel
{
    public const string SelectedApiKeyIdField = "SelectedApiKeyId";

    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly IDataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly IApplicationService _appService;
    private readonly ISharedBillingService _billingService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly BillingOptions _billingOptions;

    public SettingsModel(
        ILogger<SettingsModel> logger,
        IDataService dataService,
        ICurrentContext currentContext,
        IApplicationService appService,
        ISharedBillingService billingService,
        IPasswordlessManagementClient managementClient,
        IOptions<BillingOptions> billingOptions
        )
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _appService = appService;
        _billingService = billingService;
        _managementClient = managementClient;
        _billingOptions = billingOptions.Value;
    }

    public Models.Organization Organization { get; set; }

    public string ApplicationId { get; private set; }

    public bool PendingDelete { get; set; }

    public DateTime? DeleteAt { get; set; }

    public Application? Application { get; private set; }

    public ICollection<PlanModel> Plans { get; } = new List<PlanModel>();

    public IReadOnlyCollection<ApiKey> ApiKeys { get; private set; }

    private async Task InitializeAsync()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();
        ApplicationId = _currentContext.AppId ?? String.Empty;

        var application = Organization.Applications.FirstOrDefault(x => x.Id == ApplicationId);

        if (application == null) throw new InvalidOperationException("Application not found.");
        Application = application;
    }

    public async Task OnGet()
    {
        await InitializeAsync();

        var apiKeys = await _managementClient.GetApiKeysAsync(ApplicationId);
        ApiKeys = apiKeys
            .Select(x => ApiKey.FromDto(x, Application!))
            .Where(x => x.AllowDestructiveAction)
            .ToImmutableList();

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
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected happened. Please try again later." });
        }

        var response = await _appService.MarkApplicationForDeletionAsync(applicationId, userName);

        return response.IsDeleted ? RedirectToPage("/Organization/Overview") : RedirectToPage();
    }

    /// <summary>
    /// Cancels the deletion of the application.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Handles the plan change.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="selectedPlan"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostChangePlanAsync(string app, string selectedPlan)
    {

        var redirectUrl = await _billingService.ChangePlanAsync(app, selectedPlan);

        return Redirect(redirectUrl);
    }

    public async Task<IActionResult> OnPostLockApiKeyAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = Request.Form[SelectedApiKeyIdField].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.LockApiKeyAsync(applicationId, selectedApiKeyId);

            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to lock api key for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public async Task<IActionResult> OnPostUnlockApiKeyAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.UnlockApiKeyAsync(applicationId, selectedApiKeyId);

            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to unlock api key for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public async Task<IActionResult> OnPostDeleteApiKeyAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.DeleteApiKeyAsync(applicationId, selectedApiKeyId);

            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to delete api key for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
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

    public record ApiKey(
        string Id,
        string Value,
        string Type,
        string Scopes,
        bool IsLocked,
        DateTime? LastLockedAt,
        bool AllowDestructiveAction)
    {
        public static ApiKey FromDto(ApiKeyResponse dto, Application application)
        {
            return new ApiKey(
                dto.Id,
                dto.ApiKey,
                dto.Type.ToString(),
                string.Join(", ", dto.Scopes),
                dto.IsLocked,
                dto.LastLockedAt,
                dto.Type == ApiKeyTypes.Public ? application.ApiKey.EndsWith(dto.Id) : !application.ApiSecret.EndsWith(dto.Id));
        }
    }
}