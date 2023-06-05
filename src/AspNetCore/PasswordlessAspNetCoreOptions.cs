using Passwordless.Net;

namespace Passwordless.AspNetCore;

public class PasswordlessAspNetCoreOptions : PasswordlessOptions
{
    public string? SignInScheme { get; set; }
    public string? UserIdClaimType { get; set; }
}
