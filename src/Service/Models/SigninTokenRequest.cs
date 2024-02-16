namespace Passwordless.Service.Models;

public class SigninTokenRequest(string userId, int? timeToLive = null) : RequestBase
{
    public string UserId { get; } = userId;
    /// <summary>
    /// Time to live is the number of seconds the token has before it expires.
    /// </summary>
    public int? TimeToLive { get; } = timeToLive;
};