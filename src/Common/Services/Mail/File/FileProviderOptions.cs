namespace Passwordless.Common.Services.Mail.File;

public class FileProviderOptions : IProviderOptions
{
    public const string Provider = "file";

    public string? Path { get; set; }
}