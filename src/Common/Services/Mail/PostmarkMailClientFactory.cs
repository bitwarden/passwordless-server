using System.Net.Mail;
using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailClientFactory(PostmarkMailProviderConfiguration configuration)
{
    private static readonly Dictionary<string, ConfiguredPostmarkClient> Clients = new();

    public ConfiguredPostmarkClient GetClient(string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            return GetClient("Default");
        }

        if (Clients.TryGetValue(streamName, out var value))
        {
            return value;
        }

        var messageStream = configuration.MessageStreams?.FirstOrDefault(x => x.Name == streamName);

        if (messageStream == null)
        {
            var defaultClient = new ConfiguredPostmarkClient
            {
                Client = new PostmarkClient(configuration.DefaultConfiguration.ApiKey),
                From = new MailAddress(configuration.DefaultConfiguration.From)
            };

            Clients.TryAdd("Default", defaultClient);
            return defaultClient;
        }

        var streamClient = new ConfiguredPostmarkClient
        {
            Client = new PostmarkClient(messageStream.ApiKey),
            From = new MailAddress(messageStream.From),
            MessageStream = messageStream.Name
        };

        Clients.TryAdd(messageStream.Name, streamClient);
        return streamClient;
    }
}

public class ConfiguredPostmarkClient
{
    public required PostmarkClient Client { get; init; }
    public MailAddress? From { get; init; }
    public string? MessageStream { get; init; }
}