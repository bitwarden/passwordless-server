namespace Passwordless.Common.Services.Mail;

public record MailMessage(IEnumerable<string> To, string? From, string Subject, string TextBody, string HtmlBody, string Tag)
{
    public MailMessage(string to, string? from, string subject, string textBody, string htmlBody, string tag)
        : this(new List<string> { to }, from, subject, textBody, htmlBody, tag)
    {
    }

    public MailMessage() : this(new List<string>(), null, null, null, null, null)
    {
    }
}