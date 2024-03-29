namespace Passwordless.Service.Models;

public class SignInBeginDTO : RequestBase
{
    public string Alias { get; set; }
    public string UserId { get; init; }
    public SignInPurpose Purpose { get; set; } = SignInPurposes.SignIn;
}

public class SignInPurpose
{
    public required string Value { get; init; }
}