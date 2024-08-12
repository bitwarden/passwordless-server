namespace Passwordless.Common.Services.Mail.SendGrid;

public class SendGridProviderOptions : IProviderOptions
{
    public const string Provider = "sendgrid";

    public string ApiKey { get; set; }
}