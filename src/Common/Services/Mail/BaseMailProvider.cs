using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public abstract class BaseMailProvider : IMailProvider
{
    private readonly IEmailChannelStrategy _emailChannelStrategy;

    protected BaseMailProvider(BaseMailProviderOptions options)
    {
        _emailChannelStrategy = new EmailChannelStrategy(options);
    }

    /// <summary>
    /// Any common logic shared by multiple providers that should be executed before sending the message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task SendAsync(MailMessage message)
    {
        _emailChannelStrategy.SetSenderInfo(message);
        return Task.CompletedTask;
    }
}