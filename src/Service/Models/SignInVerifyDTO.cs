using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class SignInVerifyDTO : RequestBase
{
    public string Token { get; set; }
    public SignInPurpose Purpose { get; set; } = SignInPurposes.SignIn;
}