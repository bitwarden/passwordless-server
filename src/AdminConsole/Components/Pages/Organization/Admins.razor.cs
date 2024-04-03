using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
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

    private ModelState? DeleteAdminModelState { get; set; }
    private ModelState? InviteAdminModelState { get; set; }

    [SupplyParameterFromForm(FormName = DeleteActiveFormName)]
    public DeleteActiveFormModel DeleteActiveForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = InviteFormName)]
    public InviteFormModel InviteForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = CancelInviteFormName)]
    public CancelInviteFormModel CancelInviteForm { get; set; } = new();

    public bool CanInvite { get; set; }

    public IReadOnlyCollection<ConsoleAdmin>? ConsoleAdmins { get; set; }

    public IReadOnlyCollection<Invite>? Invites { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ConsoleAdmins = await DataService.GetConsoleAdminsAsync();
        Invites = await InvitationService.GetInvitesAsync(CurrentContext.OrgId!.Value);
        CanInvite = await DataService.CanInviteAdminAsync();
    }

    private async Task DeleteAdminAsync()
    {
        var users = await DataService.GetConsoleAdminsAsync();

        if (users is not { Count: > 1 })
        {
            DeleteAdminModelState = new ModelState("At least one admin is required in an organization.");
            return;
        }

        var user = users.FirstOrDefault(u => u.Id == DeleteActiveForm.UserId);
        if (user is null)
        {
            DeleteAdminModelState = new ModelState("User not found.");
            return;
        }

        // Delete Credentials + aliases
        await PasswordlessClient.DeleteUserAsync(user.Id);
        // Delete from admin consoles
        await UserManager.DeleteAsync(user);

        var performedBy = users.FirstOrDefault(x => x.Email == HttpContextAccessor.HttpContext!.User.GetEmail());
        if (performedBy is not null)
            EventLogger.LogDeleteAdminEvent(performedBy, user, TimeProvider.GetUtcNow().UtcDateTime);

        // if user is self
        if (user.Email == HttpContextAccessor.HttpContext!.User.GetEmail())
        {
            await SigninManager.SignOutAsync();
            NavigationManager.NavigateTo("/Account/Login");
        }

        NavigationManager.Refresh();
    }

    private async Task InviteAsync()
    {
        if (CanInvite is false)
        {
            InviteAdminModelState = new ModelState("You need to upgrade to a paid organization to invite more admins.");
            return;
        }

        if (ConsoleAdmins!.Any(x => string.Equals(x.Email, InviteForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            InviteAdminModelState = new ModelState("This e-mail is already an admin.");
            return;
        }

        var existingInvites = await InvitationService.GetInvitesAsync(CurrentContext.OrgId!.Value);

        if (existingInvites.Count >= 10)
        {
            InviteAdminModelState = new ModelState("You can only have 10 pending invites at a time.");
            return;
        }

        if (existingInvites.Any(x => string.Equals(x.ToEmail, InviteForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            InviteAdminModelState = new ModelState("There is a pending invite already for this address. Please cancel before resending.");
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

        var inviteToCancel = Invites.FirstOrDefault(x => x.HashedCode == CancelInviteForm.HashedCode);

        if (inviteToCancel is null)
        {
            InviteAdminModelState = new ModelState("Failed to find an invite to cancel.");
            return;
        }

        await InvitationService.CancelInviteAsync(inviteToCancel);

        var performedBy = await DataService.GetUserAsync();

        EventLogger.LogCancelAdminInviteEvent(
            performedBy,
            inviteToCancel.ToEmail,
            TimeProvider.GetUtcNow().UtcDateTime
        );

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

    public record ModelState(string Message);
}