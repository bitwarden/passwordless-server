using Fido2NetLib;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class AuthenticationSessionConfiguration
{
    public required AssertionOptions Options { get; set; }
    public required SignInPurpose Purpose { get; set; } = SignInPurposes.SignIn;
}