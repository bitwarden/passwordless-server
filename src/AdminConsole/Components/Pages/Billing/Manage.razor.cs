using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Components.Pages.Billing;

public partial class Manage : ComponentBase
{
    private bool _isInitialized;

    public readonly string[] ApplicationsHeaders = ["Id", "Description", "Users", "Plan", string.Empty];
    public const string ApplicationsEmptyMessage = "No applications found.";
    public readonly string[] PaymentMethodsHeaders = ["Type", "Number", "Expiration Date"];
    public const string PaymentMethodsEmptyMessage = "No payment methods found.";

    public const string UpgradeProFormName = "upgrade-pro-form";
    public const string ApplicationsFormName = "applications-form";
    public const string ChangePaymentMethodFormName = "change-payment-method-form";

    public ICollection<ApplicationModel>? Applications { get; set; }

    public IReadOnlyCollection<PaymentMethodModel>? PaymentMethods { get; private set; }

    [SupplyParameterFromForm(FormName = UpgradeProFormName)] public UpgradeProFormModel UpgradeProForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = ApplicationsFormName)] public ApplicationsFormModel ApplicationsForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = ChangePaymentMethodFormName)] public ChangePaymentMethodFormModel ChangePaymentMethodForm { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        var applications = await DataService.GetApplicationsAsync();
        Applications = applications
            .Select(x => ApplicationModel.FromEntity(x, BillingOptions.Value.Plans[x.BillingPlan]))
            .ToList();

        if (CurrentContext.Organization!.HasSubscription)
        {
            PaymentMethods = await BillingService.GetPaymentMethods(CurrentContext.Organization!.BillingCustomerId);
        }

        _isInitialized = true;
    }

    public record ApplicationModel(
        string Id,
        string Description,
        int Users,
        string Plan,
        bool CanChangePlan)
    {
        public static ApplicationModel FromEntity(Application entity, BillingPlanOptions options)
        {
            var canChangePlan = !entity.DeleteAt.HasValue;
            return new ApplicationModel(
                entity.Id,
                entity.Description,
                entity.CurrentUserCount,
                options.Ui.Label,
                canChangePlan);
        }
    }

    public async Task OnUpgradeProFormSubmittedAsync()
    {
        var redirect = await BillingService.GetRedirectToUpgradeOrganization(BillingOptions.Value.Store.Pro);
        NavigationManager.NavigateTo(redirect);
    }

    public async Task OnChangePaymentMethodFormSubmittedAsync()
    {
        var manageUrl = await BillingService.GetManagementUrl(CurrentContext.Organization!.Id);
        NavigationManager.NavigateTo(manageUrl);
    }

    public void OnApplicationsFormSubmitted()
    {
        if (ApplicationsForm.Action == "ChangePlan")
        {
            if (string.IsNullOrEmpty(ApplicationsForm.ApplicationId))
            {
                throw new ArgumentException("ApplicationId is required.");
            }
            NavigationManager.NavigateTo($"/app/{ApplicationsForm.ApplicationId}/settings");
        }
    }

    public class UpgradeProFormModel
    {
        public string Action { get; set; }
    }

    public sealed class ApplicationsFormModel
    {
        public string Action { get; set; }
        public string ApplicationId { get; set; }
    }

    public class ChangePaymentMethodFormModel
    {
        public string Action { get; set; }
    }
}