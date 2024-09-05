using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public class MailMessage
{
    public IEnumerable<string> To { get; init; }

    public string? From { get; set; }

    public string? FromDisplayName { get; set; }

    public string Subject { get; init; }

    public required string TextBody { get; init; }

    public required string HtmlBody { get; init; }

    public string Tag { get; init; }

    public Channel Channel { get; init; } = Channel.Default;

    public ICollection<string> Bcc { get; init; } = new List<string>();
}