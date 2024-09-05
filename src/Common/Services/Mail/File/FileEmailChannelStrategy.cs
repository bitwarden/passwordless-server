using System.Configuration;
using Amazon.SimpleEmailV2.Model;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.File;

public class FileEmailChannelStrategy
{
    private readonly FileMailProviderOptions _mailConfiguration;

    public FileEmailChannelStrategy(FileMailProviderOptions mailConfiguration)
    {
        _mailConfiguration = mailConfiguration;
    }

    public void SetSenderInfo(MailMessage message)
    {
        var key = _mailConfiguration.Channels.ContainsKey(message.Channel) ? message.Channel : Channel.Default;

        if (_mailConfiguration.Channels.TryGetValue(key, out var channelConfiguration))
        {
            message.From = channelConfiguration.From;
            message.FromDisplayName ??= channelConfiguration.FromName;
            return;
        }

        // 3. If there is no default channel configuration present, throw an exception.
        const string errorMessage = "No default channel configuration found to send e-mails.";
        throw new ConfigurationErrorsException(errorMessage);
    }
}