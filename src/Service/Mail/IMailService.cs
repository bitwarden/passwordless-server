using Passwordless.Service.Models;

namespace Passwordless.Service.Mail;

public interface IMailService
{
    Task SendApplicationDeletedAsync(AccountMetaInformation accountMetaInformation, DateTime deletedAt, string deletedBy);
    Task SendApplicationToBeDeletedAsync(AccountMetaInformation accountInformation, DateTime deleteAt, string deletedBy, string cancellationLink);
}