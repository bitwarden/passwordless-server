namespace Passwordless.Common.Services.Mail;

public record MailMessage(IEnumerable<string> To, string? From, string Subject, string TextBody, string HtmlBody, string Tag, string MessageType)
{
    public MailMessage(string to, string? from, string subject, string textBody, string htmlBody, string tag)
        : this(new List<string> { to }, from, subject, textBody, htmlBody, tag, string.Empty)
    {
    }

    public MailMessage() : this(new List<string>(), null, null, null, null, null, string.Empty)
    {
    }

    public ICollection<string> Bcc { get; set; } = new List<string>();
}