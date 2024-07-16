using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Settings : ComponentBase
{
    private const string SecurityFormName = "security-form";
    private const string DeleteFormName = "delete-form";

    protected override async Task OnInitializedAsync()
    {
        DeleteFormEditContext = new EditContext(DeleteForm);
        DeleteFormValidationMessageStore = new ValidationMessageStore(DeleteFormEditContext);

        var organization = await DataService.GetOrganizationWithDataAsync();
        Name = organization.Name;
        ApplicationsCount = organization.Applications.Count;

        SecurityForm ??= new SecurityFormModel { IsMagicLinksEnabled = organization.IsMagicLinksEnabled };
    }

    private async Task OnSecurityFormSubmittedAsync()
    {
        await DataService.UpdateOrganizationSecurityAsync(SecurityForm!.IsMagicLinksEnabled);
    }

    private async Task OnDeleteFormSubmittedAsync()
    {
        var username = HttpContextAccessor.HttpContext!.User.Identity?.Name ?? throw new InvalidOperationException();
        if (!string.Equals(Name, DeleteForm.NameConfirmation, StringComparison.Ordinal))
        {
            DeleteFormValidationMessageStore!.Add(() => DeleteForm.NameConfirmation, "Entered organization name does not match.");
            return;
        }

        var organization = await DataService.GetOrganizationWithDataAsync();
        var emails = organization.Admins.Select(x => x.Email!).ToList();
        await MailService.SendOrganizationDeletedAsync(organization.Name, emails, username, TimeProvider.GetUtcNow().UtcDateTime);

        if (organization.HasSubscription)
        {
            var isSubscriptionDeleted = await BillingService.CancelSubscriptionAsync(organization.BillingSubscriptionId!);
            if (!isSubscriptionDeleted)
            {
                Logger.LogError(
                    "Organization {orgId} tried to cancel subscription {subscriptionId}, but failed.",
                    organization.Name,
                    organization.BillingSubscriptionId);
                throw new Exception("Failed to cancel subscription.");
            }
        }

        var isDeleted = await DataService.DeleteOrganizationAsync();
        if (isDeleted)
        {
            await SignInManager.SignOutAsync();
        }

        NavigationManager.Refresh();
    }

    [SupplyParameterFromForm(FormName = SecurityFormName)]
    public SecurityFormModel? SecurityForm { get; set; }

    [SupplyParameterFromForm(FormName = DeleteFormName)]
    public DeleteFormModel DeleteForm { get; set; } = new();

    public EditContext? DeleteFormEditContext { get; set; }

    public ValidationMessageStore? DeleteFormValidationMessageStore { get; set; }

    /// <summary>
    /// Organization Name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the organization can be deleted.
    /// </summary>
    public bool CanDelete => ApplicationsCount == 0;

    /// <summary>
    /// The amount of active applications belonging to the organization.
    /// </summary>
    public int ApplicationsCount { get; set; }

    public class SecurityFormModel
    {
        [Required]
        public bool IsMagicLinksEnabled { get; set; }
    }

    public class DeleteFormModel
    {
        /// <summary>
        /// The organization's name which is confirmed by the end user to allow the organization to be deleted.
        /// </summary>
        [Required(ErrorMessage = "Please confirm the organization name.")]
        [MaxLength(50, ErrorMessage = "The organization name must be at most 50 characters.")]
        public string NameConfirmation { get; set; } = string.Empty;
    }
}