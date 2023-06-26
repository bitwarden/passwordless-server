using Passwordless.Net;

namespace Passwordless.AdminConsole;

public class PasswordlessClientOptions
{
    public string ApiKey { get; set; } = null!;
    public string ApiUrl { get; set; } = PasswordlessOptions.CloudApiUrl;
}
