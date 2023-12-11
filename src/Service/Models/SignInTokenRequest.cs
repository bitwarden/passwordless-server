namespace Passwordless.Service.Models;

public class SignInTokenRequest(string userId) : RequestBase
{
    public string UserId { get; } = userId;
};