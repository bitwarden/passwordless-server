using System.Configuration;
using System.Net.Mail;
using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailClientFactory(PostmarkMailProviderConfiguration configuration)
{
    private static readonly Dictionary<string, ConfiguredPostmarkClient> Clients = new();

    public ConfiguredPostmarkClient GetClient(string name)
    {
        var clientName = string.IsNullOrWhiteSpace(name)
            ? configuration.DefaultConfiguration.Name
            : name;

        if (Clients.TryGetValue(clientName, out var value))
        {
            return value;
        }

        if (clientName == configuration.DefaultConfiguration.Name)
        {
            var defaultClient = GetConfiguredClient(configuration.DefaultConfiguration);

            Clients.TryAdd(defaultClient.Name, defaultClient);
            return defaultClient;
        }

        var messageStream = configuration.MessageStreams?.FirstOrDefault(x => x.Name == clientName);

        if (messageStream == null) throw new ConfigurationErrorsException($"The {clientName} message stream is not properly configured for Postmark.");

        var configuredClient = GetConfiguredClient(messageStream);

        Clients.TryAdd(configuredClient.Name, configuredClient);

        return configuredClient;
    }

    private static ConfiguredPostmarkClient GetConfiguredClient(PostmarkClientConfiguration config) =>
        new()
        {
            Client = new PostmarkClient(config.ApiKey),
            From = new MailAddress(config.From),
            Name = config.Name
        };
}

public class ConfiguredPostmarkClient
{
    public required PostmarkClient Client { get; init; }
    public MailAddress? From { get; init; }
    public string Name { get; init; } = string.Empty;
}