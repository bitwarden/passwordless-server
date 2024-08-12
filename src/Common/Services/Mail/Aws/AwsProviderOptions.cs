namespace Passwordless.Common.Services.Mail.Aws;

public class AwsProviderOptions : BaseProviderOptions
{
    public const string Provider = "aws";

    public string AccessKey { get; set; }

    public string SecretKey { get; set; }

    public string Region { get; set; }
}