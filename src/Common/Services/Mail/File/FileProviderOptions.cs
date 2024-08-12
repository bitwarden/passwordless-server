namespace Passwordless.Common.Services.Mail.File;

public class FileProviderOptions : BaseProviderOptions
{
    public const string Provider = "file";

    public FileProviderOptions()
    {
        Name = Provider;
    }

    public string? Path { get; set; }
}