using Fido2NetLib;

namespace Passwordless.Service.Models;

public class SignInCompleteDTO : RequestBase
{
    public required AuthenticatorAssertionRawResponse Response { get; set; }
    public required string Session { get; set; }
}