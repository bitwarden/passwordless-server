namespace AdminConsole.Services.Mail;

public record MailMessage(string To, string? From, string Subject, string TextBody, string HtmlBody, string Tag)
{
    public MailMessage() : this(null, null, null, null, null, null)
    {
    }
}