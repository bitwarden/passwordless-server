using System.Net.Mail;
using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailProviderConfiguration
{
    public required PostmarkClientConfiguration DefaultConfiguration { get; init; }
    public List<PostmarkClientConfiguration> MessageStreams { get; init; } = [];
}

public class PostmarkClientConfiguration
{
    public string Name { get; init; }
    public required string ApiKey { get; init; }
    public string From { get; init; }

    public ConfiguredPostmarkClient GetConfiguredClient() =>
        new()
        {
            Client = new PostmarkClient(ApiKey),
            From = new MailAddress(From)
        };
}

public class ConfiguredPostmarkClient
{
    public required PostmarkClient Client { get; init; }
    public MailAddress? From { get; init; }
}