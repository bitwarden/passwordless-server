namespace AdminConsole.Services.Mail;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    public Task SendAsync(MailMessage message)
    {
        string msg = message.TextBody;
        Console.WriteLine(msg);
        File.AppendAllText("mail.txt", msg);

        return Task.CompletedTask;
    }
}