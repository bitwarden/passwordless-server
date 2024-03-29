using Fido2NetLib;

namespace Passwordless.Service.Models;

public class AuthenticationSessionConfiguration
{
    public required AssertionOptions Options { get; set; }
    public required SignInPurpose Purpose { get; set; } = SignInPurposes.SignIn;
}