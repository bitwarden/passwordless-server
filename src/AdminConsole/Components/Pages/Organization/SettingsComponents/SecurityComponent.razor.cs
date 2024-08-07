using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Passwordless.AdminConsole.Components.Pages.Organization.SettingsComponents;

public partial class SecurityComponent : ComponentBase
{
    private const string SecurityFormName = "security-form";

    [Parameter]
    public bool IsMagicLinksEnabled { get; set; }

    [SupplyParameterFromForm(FormName = SecurityFormName)]
    public SecurityFormModel? SecurityForm { get; set; }

    public EditContext? SecurityFormEditContext { get; set; }

    public ValidationMessageStore? SecurityFormValidationMessageStore { get; set; }

    protected override void OnInitialized()
    {
        // If the form is being submitted, we need to initialize the form model
        // Else, we need to initialize the form model with the current state from the component's incoming parameter.
        if (HttpContextAccessor.HttpContext!.Request.HasFormContentType &&
            HttpContextAccessor.HttpContext.Request.Form["_handler"].ToString() == SecurityFormName)
        {
            SecurityForm ??= new();
        }
        else
        {
            SecurityForm ??= new SecurityFormModel { IsMagicLinksEnabled = IsMagicLinksEnabled };
        }

        SecurityFormEditContext = new EditContext(SecurityForm);
        SecurityFormValidationMessageStore = new ValidationMessageStore(SecurityFormEditContext);
    }

    private async Task OnSecurityFormSubmittedAsync()
    {
        if (!SecurityForm!.IsMagicLinksEnabled)
        {
            var canDisableMagicLinks = await AdminService.CanDisableMagicLinksAsync();
            if (!canDisableMagicLinks)
            {
                SecurityFormValidationMessageStore!.Add(() => SecurityForm.IsMagicLinksEnabled, "Cannot disable magic links because there are admins without passkeys.");
                SecurityFormEditContext!.NotifyValidationStateChanged();
                SecurityForm.IsMagicLinksEnabled = IsMagicLinksEnabled;
                return;
            }
        }

        await DataService.UpdateOrganizationSecurityAsync(SecurityForm!.IsMagicLinksEnabled);
    }

    public class SecurityFormModel
    {
        [Required]
        public bool IsMagicLinksEnabled { get; set; }
    }
}