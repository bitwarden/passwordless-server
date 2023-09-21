namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Used to send a mail message
/// </summary>
public interface IMailProvider
{
    Task SendAsync(MailMessage message);
}