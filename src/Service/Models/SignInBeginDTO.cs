namespace Passwordless.Service.Models;

public class SignInBeginDTO : RequestBase
{
    public required string Alias { get; set; }
    public required string UserId { get; set; }
    public required string UserVerification { get; set; }
}