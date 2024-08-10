namespace Passwordless.Common.Services.Mail.File;

public class FileMailProviderConfiguration : IMailProviderConfiguration
{
    public const string Name = "File";

    public string GetName() => Name;

    public string? Path { get; set; }
}