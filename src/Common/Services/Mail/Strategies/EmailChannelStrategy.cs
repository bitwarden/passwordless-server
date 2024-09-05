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
        // 1. If the message has a channel, use the channel's configuration.
        if (_mailConfiguration.Channels.TryGetValue(message.Channel, out var channelConfiguration))
        {
            message.From = channelConfiguration.From;
            message.FromDisplayName ??= channelConfiguration.FromName;
            return;
        }

        // 2. If the message does not have a channel, use the default channel configuration.
        if (_mailConfiguration.Channels.TryGetValue(Channel.Default, out var defaultChannelConfiguration))
        {
            message.From = defaultChannelConfiguration.From;
            message.FromDisplayName ??= defaultChannelConfiguration.FromName;
            return;
        }

        // 3. If there is no default channel configuration present, throw an exception.
        const string errorMessage = "No default channel configuration found to send e-mails.";
        throw new ConfigurationErrorsException(errorMessage);
    }
}