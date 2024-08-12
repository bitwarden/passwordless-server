namespace Passwordless.Common.Services.Mail.SendGrid;

public class SendGridProviderOptions : BaseProviderOptions
{
    public const string Provider = "sendgrid";

    public string ApiKey { get; set; }
}