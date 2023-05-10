namespace Passwordless.Service.Models;

public class FidoRegistrationBeginDTO : RequestBase
{
    public string Token { get; set; }
}