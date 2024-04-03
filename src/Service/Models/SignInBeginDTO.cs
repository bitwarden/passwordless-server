using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class SignInBeginDTO : RequestBase
{
    public string Alias { get; set; }
    public string UserId { get; init; }
    public SignInPurpose Purpose { get; set; } = SignInPurposes.SignIn;
}