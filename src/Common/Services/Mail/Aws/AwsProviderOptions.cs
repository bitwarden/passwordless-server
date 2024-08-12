namespace Passwordless.Common.Services.Mail.Aws;

public class AwsProviderOptions : IProviderOptions
{
    public const string Provider = "aws";

    public string AccessKey { get; set; }

    public string SecretAccessKey { get; set; }

    public string Region { get; set; }
}