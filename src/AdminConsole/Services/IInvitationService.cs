using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Services;

public interface IInvitationService
{
    Task SendInviteAsync(string toEmail, int TargetOrgId, string TargetOrgName, string fromEmail, string fromName);
    Task<List<Invite>> GetInvitesAsync(int orgId);
    Task CancelInviteAsync(string hashedCode);
    Task<Invite> GetInviteFromRawCodeAsync(string code);
    Task<bool> ConsumeInviteAsync(Invite inv);
}