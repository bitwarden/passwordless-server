using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Validation;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class CreateApplication : ComponentBase
{
    public const string FormName = "create-application-form";

    [SupplyParameterFromForm(FormName = FormName)]
    public CreateApplicationForm? Form { get; set; }

    public EditContext? FormEditContext { get; set; }

    public ValidationMessageStore? FormValidationMessageStore { get; set; }

    public ICollection<AvailablePlan> AvailablePlans { get; private set; } = new List<AvailablePlan>();

    protected override async Task OnInitializedAsync()
    {
        var canCreateApplications = await DataService.AllowedToCreateApplicationAsync();
        if (!canCreateApplications)
        {
            NavigationManager.NavigateTo("/billing/manage");
        }

        Form ??= new CreateApplicationForm();
        FormEditContext = new EditContext(Form);
        FormValidationMessageStore = new ValidationMessageStore(FormEditContext);

        if (CurrentContext.Organization!.HasSubscription)
        {
            AvailablePlans.Add(new AvailablePlan(BillingOptions.Value.Store.Pro, BillingOptions.Value.Plans[BillingOptions.Value.Store.Pro].Ui.Label));
            AvailablePlans.Add(new AvailablePlan(BillingOptions.Value.Store.Enterprise, BillingOptions.Value.Plans[BillingOptions.Value.Store.Enterprise].Ui.Label));
        }
        else
        {
            Form.Plan = BillingOptions.Value.Store.Free;
        }
    }



    public async Task OnSubmittedAsync()
    {
        Application app = new()
        {
            Name = Form!.Name,
            Description = Form.Description,
            CreatedAt = DateTime.UtcNow,
            OrganizationId = HttpContextAccessor.HttpContext!.User.GetOrgId()!.Value
        };

        string email = HttpContextAccessor.HttpContext.User.GetEmail();

        if (!await DataService.AllowedToCreateApplicationAsync())
        {
            FormValidationMessageStore!.Add(() => Form.Plan, "You have reached the maximum number of applications for your organization. Please upgrade to a paid organization");
            return;
        }

        // Attach a plan
        app.BillingPlan = Form.Plan;

        if (Form.Plan != BillingOptions.Value.Store.Free)
        {
            var subItem = await BillingService.CreateSubscriptionItem(CurrentContext.Organization!, Form.Plan);

            app.BillingSubscriptionItemId = subItem.subscriptionItemId;
            app.BillingPriceId = subItem.priceId;
        }

        CreateAppResultDto res;
        try
        {
            var features = BillingOptions.Value.Plans[Form.Plan].Features;
            var newAppOptions = new CreateAppDto
            {
                AdminEmail = email,
                EventLoggingIsEnabled = features.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
                MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota,
                MaxUsers = features.MaxUsers,
                AllowAttestation = features.AllowAttestation
            };
            res = await ManagementClient.CreateApplicationAsync(newAppOptions);
        }
        catch (Exception)
        {
            FormValidationMessageStore!.Add(() => Form.Name, "Failed to create your application");
            return;
        }

        if (string.IsNullOrEmpty(res.ApiKey1))
        {
            FormValidationMessageStore!.Add(() => Form.Name, res.Message);
            return;
        }

        // TODO: Get "Admin Console Keys" and "Real Keys"
        app.Id = res.AppId;
        app.ApiKey = res.ApiKey2;
        app.ApiSecret = res.ApiSecret2;
        app.ApiUrl = PasswordlessOptions.Value.ApiUrl!;
        app.Onboarding = new Onboarding
        {
            ApiKey = res.ApiKey1,
            ApiSecret = res.ApiSecret1,
            SensitiveInfoExpireAt = DateTime.UtcNow.AddDays(7)
        };

        await ApplicationService.CreateApplicationAsync(app);

        NavigationManager.NavigateTo($"/app/{app.Id}/onboarding/get-started");
    }

    public class CreateApplicationForm
    {
        [Required, MaxLength(60), MinLength(3), NoForbiddenContent]
        public string Name { get; set; }

        [Required, MaxLength(120), MinLength(3), NoForbiddenContent]
        public string Description { get; set; }

        [Required]
        public string Plan { get; set; }
    }

    public record AvailablePlan(string Id, string Label);
}