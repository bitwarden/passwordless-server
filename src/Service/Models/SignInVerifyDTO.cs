namespace Passwordless.Service.Models;

public class SignInVerifyDTO : RequestBase
{
    public required string Token { get; set; }
}