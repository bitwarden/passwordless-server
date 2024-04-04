using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Admins : ComponentBase
{
    private const string ActiveEmptyMessage = "No admins found.";
    private const string InvitationsEmptyMessage = "No invitations found.";

    private readonly IReadOnlyCollection<string> _activeColumnHeaders = new List<string> { "Admin", "E-mail", "Action" };
    private readonly IReadOnlyCollection<string> _invitationColumnHeaders = new List<string> { "Recipient", "Invited", "Status", "Sender", "Action" };

    private const string DeleteActiveFormName = "delete-active-form";
    private const string InviteFormName = "invite-form";
    private const string CancelInviteFormName = "cancel-invite-form";

    [SupplyParameterFromForm(FormName = DeleteActiveFormName)]
    public DeleteActiveFormModel DeleteActiveForm { get; set; } = new();

    public EditContext? DeleteActiveFormEditContext { get; set; }

    public ValidationMessageStore? DeleteActiveFormValidationMessageStore { get; set; }

    [SupplyParameterFromForm(FormName = InviteFormName)]
    public InviteFormModel InviteForm { get; set; } = new();

    public EditContext? InviteFormEditContext { get; set; }

    public ValidationMessageStore? InviteFormValidationMessageStore { get; set; }

    [SupplyParameterFromForm(FormName = CancelInviteFormName)]
    public CancelInviteFormModel CancelInviteForm { get; set; } = new();

    public EditContext? CancelInviteFormEditContext { get; set; }

    public ValidationMessageStore? CancelInviteFormValidationMessageStore { get; set; }

    public bool CanInvite { get; set; }

    public IReadOnlyCollection<ConsoleAdmin>? ConsoleAdmins { get; set; }

    public IReadOnlyCollection<Invite>? Invites { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DeleteActiveFormEditContext = new EditContext(DeleteActiveForm);
        DeleteActiveFormValidationMessageStore = new ValidationMessageStore(DeleteActiveFormEditContext);

        InviteFormEditContext = new EditContext(InviteForm);
        InviteFormValidationMessageStore = new ValidationMessageStore(InviteFormEditContext);

        CancelInviteFormEditContext = new EditContext(CancelInviteForm);
        CancelInviteFormValidationMessageStore = new ValidationMessageStore(CancelInviteFormEditContext);

        ConsoleAdmins = await DataService.GetConsoleAdminsAsync();
        Invites = await InvitationService.GetInvitesAsync(CurrentContext.OrgId!.Value);
        CanInvite = await DataService.CanInviteAdminAsync();
    }

    private async Task DeleteAdminAsync()
    {
        if (ConsoleAdmins is not { Count: > 1 })
        {
            DeleteActiveFormValidationMessageStore!.Add(() => DeleteActiveForm.UserId, "At least one admin is required in an organization.");
        }

        var user = ConsoleAdmins!.FirstOrDefault(u => u.Id == DeleteActiveForm.UserId);
        if (user is null)
        {
            DeleteActiveFormValidationMessageStore!.Add(() => DeleteActiveForm.UserId, "User not found.");
        }

        if (DeleteActiveFormEditContext!.GetValidationMessages().Any())
        {
            return;
        }

        var performedBy = ConsoleAdmins!.First(x => x.Email == HttpContextAccessor.HttpContext!.User.GetEmail());
        EventLogger.LogDeleteAdminEvent(performedBy, user!, TimeProvider.GetUtcNow().UtcDateTime);

        await PasswordlessClient.DeleteUserAsync(user!.Id);
        await UserManager.DeleteAsync(user);

        // if user is self
        if (user.Email == HttpContextAccessor.HttpContext!.User.GetEmail())
        {
            await SigninManager.SignOutAsync();
            NavigationManager.NavigateTo("/Account/Login");
        }

        NavigationManager.Refresh();
    }

    private async Task OnValidInviteAsync()
    {
        if (CanInvite is false)
        {
            InviteFormValidationMessageStore!.Add(() => InviteForm.Email, "You need to upgrade to a paid organization to invite more admins.");
            InviteFormEditContext!.NotifyValidationStateChanged();
        }

        if (ConsoleAdmins!.Any(x => string.Equals(x.Email, InviteForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            InviteFormValidationMessageStore!.Add(() => InviteForm.Email, "This e-mail is already an admin.");
            InviteFormEditContext!.NotifyValidationStateChanged();
        }

        if (Invites!.Count >= 10)
        {
            InviteFormValidationMessageStore!.Add(() => InviteForm.Email, "You can only have 10 pending invites at a time.");
            InviteFormEditContext!.NotifyValidationStateChanged();
        }

        if (Invites.Any(x => string.Equals(x.ToEmail, InviteForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            InviteFormValidationMessageStore!.Add(() => InviteForm.Email, "There is a pending invite already for this address. Please cancel before resending.");
            InviteFormEditContext!.NotifyValidationStateChanged();
        }

        if (InviteFormEditContext!.GetValidationMessages().Any())
        {
            return;
        }

        var user = await DataService.GetUserAsync();
        await InvitationService.SendInviteAsync(InviteForm.Email, CurrentContext.Organization!.Id, CurrentContext.Organization!.Name, user.Email!, user.Name);
        EventLogger.LogInviteAdminEvent(user, InviteForm.Email, TimeProvider.GetUtcNow().UtcDateTime);

        NavigationManager.Refresh();
    }

    public async Task CancelInviteAsync()
    {
        Invites = await InvitationService.GetInvitesAsync(CurrentContext.OrgId!.Value);

        var invite = Invites.FirstOrDefault(x => x.HashedCode == CancelInviteForm.HashedCode);

        if (invite is null)
        {
            CancelInviteFormValidationMessageStore!.Add(() => CancelInviteForm.HashedCode, "Failed to find an invite to cancel.");
            return;
        }

        await InvitationService.CancelInviteAsync(invite);

        var performedBy = await DataService.GetUserAsync();

        EventLogger.LogCancelAdminInviteEvent(performedBy, invite.ToEmail, TimeProvider.GetUtcNow().UtcDateTime);

        NavigationManager.Refresh();
    }

    public class DeleteActiveFormModel
    {
        public string UserId { get; set; }
    }

    public class InviteFormModel
    {
        [Required, EmailAddress, MaxLength(50)]
        public string Email { get; set; }
    }

    public class CancelInviteFormModel
    {
        public string HashedCode { get; set; }
    }
}