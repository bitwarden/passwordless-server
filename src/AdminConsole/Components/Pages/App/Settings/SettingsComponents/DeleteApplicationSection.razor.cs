using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class DeleteApplicationSection : ComponentBase
{
    public const string Unknown = "unknown";
    public const string CancelDeleteFormName = "cancel-delete-application-form";
    public const string DeleteFormName = "delete-application-form";

    public bool? CanDeleteImmediately { get; set; }

    [Parameter]
    public required Application Application { get; set; }

    [SupplyParameterFromForm(FormName = DeleteFormName)]
    public DeleteFormModel? DeleteForm { get; set; }

    public EditContext? DeleteFormContext { get; set; }

    public ValidationMessageStore? DeleteFormValidationMessageStore { get; set; }

    [SupplyParameterFromForm(FormName = CancelDeleteFormName)]
    public CancelDeleteFormModel? CancelDeleteForm { get; set; }

    public EditContext? CancelDeleteFormContext { get; set; }

    public ValidationMessageStore? CancelDeleteFormValidationMessageStore { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DeleteForm ??= new DeleteFormModel();
        DeleteFormContext = new EditContext(DeleteForm);
        DeleteFormValidationMessageStore = new ValidationMessageStore(DeleteFormContext);

        CancelDeleteForm ??= new CancelDeleteFormModel();
        CancelDeleteFormContext = new EditContext(CancelDeleteForm);
        CancelDeleteFormValidationMessageStore = new ValidationMessageStore(CancelDeleteFormContext);

        if (!Application.DeleteAt.HasValue)
        {
            CanDeleteImmediately = await AppService.CanDeleteApplicationImmediatelyAsync(Application.Id);
        }
    }

    private async Task OnDeleteValidSubmittedAsync()
    {
        if (DeleteForm!.NameConfirmation != Application.Name)
        {
            DeleteFormValidationMessageStore!.Add(() => DeleteForm.NameConfirmation, "Name confirmation does not match.");
            return;
        }

        var appId = Application.Id;
        var userName = HttpContextAccessor.HttpContext!.User.Identity!.Name ?? Unknown;

        if (userName == Unknown)
        {
            Logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", appId, userName);
            const string message = "Something unexpected happened.";
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode(message)}");
            return;
        }

        try
        {
            var response = await AppService.MarkDeleteApplicationAsync(appId, userName);

            if (response.IsDeleted)
            {
                NavigationManager.NavigateTo("/Organization/Overview");
            }
            else
            {
                NavigationManager.Refresh();
            }
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Failed to delete application: {appName}.", appId);
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode(ex.Message)}");
        }
    }

    private async Task OnCancelDeleteValidSubmittedAsync()
    {
        if (!Application.DeleteAt.HasValue)
        {
            CancelDeleteFormValidationMessageStore!.Add(() => CancelDeleteForm!, "Application is not marked for deletion.");
            return;
        }

        try
        {
            await AppService.CancelDeletionForApplicationAsync(Application.Id);
            NavigationManager.Refresh();
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError("Failed to cancel application deletion for application: {appId}", Application.Id);
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode(ex.Message)}");
        }
    }

    public class DeleteFormModel
    {
        public string NameConfirmation { get; set; } = string.Empty;
    }

    public class CancelDeleteFormModel;
}