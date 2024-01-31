using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailProvider(PostmarkMailProviderConfiguration configuration) : IMailProvider
{
    private readonly Dictionary<string, ConfiguredPostmarkClient> _clients = configuration.MessageStreams?
        .ToDictionary(x => x.Name, x => x.GetConfiguredClient()) ?? [];
    private readonly ConfiguredPostmarkClient _defaultClient = configuration.DefaultConfiguration.GetConfiguredClient();

    public async Task SendAsync(MailMessage message)
    {
        var configuredClient = _clients.GetValueOrDefault(message.MessageType) ?? _defaultClient;

        PostmarkMessage pm = new PostmarkMessage
        {
            To = string.Join(',', message.To),
            From = configuredClient.From?.ToString() ?? message.From,
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            Tag = message.Tag,
            Bcc = message.Bcc.Count != 0 ? string.Join(',', message.Bcc) : null,
            MessageStream = configuredClient == _defaultClient ? null : message.MessageType
        };

        IEnumerable<PostmarkResponse>? res = await configuredClient.Client.SendMessagesAsync(pm);
    }
}