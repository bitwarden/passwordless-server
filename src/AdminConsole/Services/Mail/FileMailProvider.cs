namespace AdminConsole.Services.Mail;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    public Task SendAsync(MailMessage message)
    {
        string msg = message.TextBody;
        Console.WriteLine("> Sent email to mail.md");
        msg = "# New message " + DateTime.Now + Environment.NewLine + Environment.NewLine + msg + Environment.NewLine + Environment.NewLine;
        File.AppendAllText("mail.md", msg);

        return Task.CompletedTask;
    }
}