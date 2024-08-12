namespace Passwordless.Common.Services.Mail;

public class MailMessage
{
    public MailMessage(IEnumerable<string> to, string? from, string subject, string textBody, string htmlBody, string tag, string messageType, string fromDisplayName)
    {
        To = to;
        From = from;
        Subject = subject;
        TextBody = textBody;
        HtmlBody = htmlBody;
        Tag = tag;
        MessageType = messageType;
        FromDisplayName = fromDisplayName;
    }

    public MailMessage(IEnumerable<string> to, string? from, string subject, string textBody, string htmlBody, string tag, string messageType)
        : this(to, from, subject, textBody, htmlBody, tag, messageType, string.Empty)
    {
    }

    public MailMessage(string to, string? from, string subject, string textBody, string htmlBody, string tag)
        : this(new List<string> { to }, from, subject, textBody, htmlBody, tag, string.Empty)
    {
    }

    public MailMessage()
    {
    }

    public IEnumerable<string> To { get; set; }

    public string? From { get; set; }

    public string FromDisplayName { get; set; }

    public string Subject { get; set; }

    public string TextBody { get; set; }

    public string HtmlBody { get; set; }

    public string Tag { get; set; }

    public string MessageType { get; set; }

    public ICollection<string> Bcc { get; set; } = new List<string>();
}