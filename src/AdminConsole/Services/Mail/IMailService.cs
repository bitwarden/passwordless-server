using AdminConsole.Identity;

namespace AdminConsole.Services.Mail;

/// <summary>
/// Used to generate a mail message from input and will call the mail provider to send the message
/// </summary>
public interface IMailService
{
    Task SendPasswordlessSignInAsync(string returnUrl, string token, string email);
    Task SendInviteAsync(Invite inv, string link);
    Task SendEmailIsAlreadyInUseAsync(string email);
}