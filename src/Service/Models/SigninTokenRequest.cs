namespace Passwordless.Service.Models;

public class SigninTokenRequest(string userId) : RequestBase
{
    public string UserId { get; } = userId;
};