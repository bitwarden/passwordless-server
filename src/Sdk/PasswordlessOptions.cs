namespace Passwordless.Net;

public class PasswordlessOptions
{
    public const string CloudApiUrl = "https://v4.passwordless.dev";

    public string ApiKey { get; set; } = default!;
    public string ApiUrl { get; set; } = CloudApiUrl;
    public string ApiSecret { get; set; } = default!;
}