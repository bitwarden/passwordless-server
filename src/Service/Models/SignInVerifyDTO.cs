namespace Passwordless.Service.Models;

public class SignInVerifyDTO : RequestBase
{
    public string Token { get; set; }
}