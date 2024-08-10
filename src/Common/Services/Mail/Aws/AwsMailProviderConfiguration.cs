namespace Passwordless.Common.Services.Mail.Aws;

public class AwsMailProviderConfiguration : IMailProviderConfiguration
{
    public const string Name = "Aws";

    public string GetName() => Name;

    public string AccessKey { get; set; }

    public string SecretAccessKey { get; set; }

    public string Region { get; set; }
}