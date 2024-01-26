namespace Passwordless.Common.Services.Mail;

public class PostmarkMailProviderConfiguration
{
    public PostmarkClientConfiguration DefaultConfiguration { get; init; }
    public List<PostmarkClientConfiguration>? MessageStreams { get; init; } = [];
}

public class PostmarkClientConfiguration
{
    public string Name { get; init; }
    public string ApiKey { get; init; }
    public string From { get; init; }
}