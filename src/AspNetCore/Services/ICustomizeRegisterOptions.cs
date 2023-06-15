using Passwordless.Net;

namespace Passwordless.AspNetCore.Services;

public interface ICustomizeRegisterOptions
{
    Task CustomizeAsync(CustomizeRegisterOptionsContext context, CancellationToken cancellationToken);
}

public sealed class CustomizeRegisterOptionsContext
{
    public bool NewUser { get; }
    public RegisterOptions? Options { get; set; }

    internal CustomizeRegisterOptionsContext(bool newUser, RegisterOptions options)
    {
        NewUser = newUser;
        Options = options;
    }
}