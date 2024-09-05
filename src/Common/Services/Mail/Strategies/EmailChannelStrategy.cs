using System.Configuration;

namespace Passwordless.Common.Services.Mail.Strategies;

public class EmailChannelStrategy : IEmailChannelStrategy
{
    private readonly BaseMailProviderOptions _mailConfiguration;

    public EmailChannelStrategy(BaseMailProviderOptions mailConfiguration)
    {
        _mailConfiguration = mailConfiguration;
    }

    public void SetSenderInfo(MailMessage message)
    {
        if (_mailConfiguration.Channels.TryGetValue(message.Channel, out var channelConfiguration))
        {
            message.From = channelConfiguration.From;
            message.FromDisplayName ??= channelConfiguration.FromName;
            return;
        }

        const string errorMessage = "No channel configuration found to send e-mails.";
        throw new ConfigurationErrorsException(errorMessage);
    }
}