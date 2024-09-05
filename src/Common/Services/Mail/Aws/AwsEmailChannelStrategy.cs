using System.Configuration;
using Amazon.SimpleEmailV2.Model;
using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.Aws;

public class AwsEmailChannelStrategy : IAwsEmailChannelStrategy
{
    private readonly AwsMailProviderOptions _mailConfiguration;

    public AwsEmailChannelStrategy(AwsMailProviderOptions mailConfiguration)
    {
        _mailConfiguration = mailConfiguration;
    }

    public void SetSenderInfo(SendEmailRequest message, Channel channel)
    {
        var key = _mailConfiguration.Channels.ContainsKey(channel) ? channel : Channel.Default;

        if (_mailConfiguration.Channels.TryGetValue(key, out var channelConfiguration))
        {
            if (channelConfiguration.ConfigurationSet != null)
            {
                message.ConfigurationSetName = channelConfiguration.ConfigurationSet;
            }
            return;
        }

        // 3. If there is no default channel configuration present, throw an exception.
        const string errorMessage = "No default channel configuration found to send e-mails.";
        throw new ConfigurationErrorsException(errorMessage);
    }
}