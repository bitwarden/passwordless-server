namespace Passwordless.Common.Services.Mail.SendGrid;

public class SendGridMailProviderOptions : BaseMailProviderOptions
{
    public const string Provider = "sendgrid";

    public SendGridMailProviderOptions()
    {
        Name = Provider;
    }

    public string ApiKey { get; set; }
}