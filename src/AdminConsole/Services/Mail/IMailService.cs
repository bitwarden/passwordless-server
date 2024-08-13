using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.Mail;

/// <summary>
/// Used to generate a mail message from input and will call the mail provider to send the message
/// </summary>
public interface IMailService
{
    Task SendInviteAsync(Invite inv, string link);
    Task SendEmailIsAlreadyInUseAsync(string email);
    Task SendMagicLinksDisabledAsync(string organizationName, string email);
    Task SendOrganizationDeletedAsync(string organizationName, IEnumerable<string> emails, string deletedBy, DateTime deletedAt);
    Task SendApplicationDeletedAsync(Application application, DateTime deletedAt, string deletedBy, ICollection<string> emails);
    Task SendApplicationToBeDeletedAsync(Application application, string deletedBy, string cancellationLink, ICollection<string> emails);
}