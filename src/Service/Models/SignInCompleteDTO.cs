using Fido2NetLib;

namespace Passwordless.Service.Models;

public class SignInCompleteDTO : RequestBase
{
    public AuthenticatorAssertionRawResponse Response { get; set; }
    public string Session { get; set; }
}