using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailProvider(PostmarkMailClientFactory factory) : IMailProvider
{
    public async Task SendAsync(MailMessage message)
    {
        var configuredClient = factory.GetClient(message.MessageType);

        PostmarkMessage pm = new PostmarkMessage
        {
            To = string.Join(',', message.To),
            From = configuredClient.From?.ToString() ?? message.From,
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            Tag = message.Tag,
            Bcc = message.Bcc.Any() ? string.Join(',', message.Bcc) : null,
            MessageStream = configuredClient.MessageStream
        };

        IEnumerable<PostmarkResponse>? res = await configuredClient.Client.SendMessagesAsync(pm);
    }
}