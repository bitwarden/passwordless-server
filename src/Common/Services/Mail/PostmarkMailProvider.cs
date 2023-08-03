using PostmarkDotNet;

namespace Passwordless.Common.Services.Mail;

public class PostmarkMailProvider : IMailProvider
{
    private readonly PostmarkClient _client;
    private readonly string? _fromEmail;

    public PostmarkMailProvider(IConfiguration configuration)
    {
        IConfigurationSection mailOptions = configuration.GetSection("Mail");
        IConfigurationSection postmarkOptions = mailOptions.GetSection("Postmark");
        string? apiKey = postmarkOptions.GetValue<string>("apiKey");
        _fromEmail = postmarkOptions.GetValue<string>("From", null!);
        _client = new PostmarkClient(apiKey);
    }

    public async Task SendAsync(MailMessage message)
    {
        PostmarkMessage pm = new PostmarkMessage
        {
            To = string.Join(',', message.To),
            From = _fromEmail ?? message.From,
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            Tag = message.Tag
        };

        IEnumerable<PostmarkResponse>? res = await _client.SendMessagesAsync(pm);
    }
}