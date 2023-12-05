namespace Passwordless.Service.Models;

public class FidoRegistrationBeginDTO : RequestBase
{
    public required string Token { get; set; }
}