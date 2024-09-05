namespace Passwordless.Common.Services.Mail.Strategies;

public interface IEmailChannelStrategy
{
    void SetSenderInfo(MailMessage message);
}