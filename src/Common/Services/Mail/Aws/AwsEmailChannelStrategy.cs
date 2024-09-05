using System.Configuration;
using Amazon.SimpleEmailV2.Model;
using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.Aws;

public class AwsEmailChannelStrategy
{
    private readonly AwsMailProviderOptions _mailConfiguration;

    public AwsEmailChannelStrategy(AwsMailProviderOptions mailConfiguration)
    {
        _mailConfiguration = mailConfiguration;
    }

    public SendEmailRequest SetSenderInfo(MailMessage message)
    {
        var key = _mailConfiguration.Channels.ContainsKey(message.Channel) ? message.Channel : Channel.Default;

        if (_mailConfiguration.Channels.TryGetValue(key, out var channelConfiguration))
        {
            var awsChannelConfiguration = channelConfiguration;

            var fromName = message.FromDisplayName ?? awsChannelConfiguration.FromName;
            var from = awsChannelConfiguration.From;

            var providerMessage = new SendEmailRequest
            {
                FromEmailAddress = $"{fromName} <{from}>"
            };

            if (awsChannelConfiguration.ConfigurationSet != null)
            {
                providerMessage.ConfigurationSetName = awsChannelConfiguration.ConfigurationSet;
            }
            return providerMessage;
        }

        // 3. If there is no default channel configuration present, throw an exception.
        const string errorMessage = "No default channel configuration found to send e-mails.";
        throw new ConfigurationErrorsException(errorMessage);
    }
}