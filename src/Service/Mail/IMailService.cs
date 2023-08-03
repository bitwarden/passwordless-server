using Passwordless.Service.Models;

namespace Passwordless.Service.Mail;

public interface IMailService
{
    Task SendApplicationDeletedAsync(AccountMetaInformation accountInformation, string deletedBy);
    Task SendApplicationToBeDeletedAsync(AccountMetaInformation accountInformation, string deletedBy);
}