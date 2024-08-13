namespace Passwordless.Common.Services.Mail.File;

public class FileProviderOptions : BaseProviderOptions
{
    public const string Provider = "file";
    public const string DefaultPath = "../mail.md";

    public FileProviderOptions()
    {
        Name = Provider;
    }

    public string Path { get; set; } = DefaultPath;
}