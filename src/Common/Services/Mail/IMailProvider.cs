namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Used to send a mail message
/// </summary>
public interface IMailProvider
{
    /// <summary>
    /// Sends a mail message
    /// </summary>
    /// <param name="message"></param>
    Task SendAsync(MailMessage message);
}