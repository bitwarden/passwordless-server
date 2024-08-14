namespace Passwordless.Common.Services.Mail.SendGrid;

public class SendGridMailProviderOptions : BaseMailProviderOptions
{
    public const string Provider = "sendgrid";

    public string ApiKey { get; set; }
}