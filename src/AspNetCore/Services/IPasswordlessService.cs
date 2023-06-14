using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Passwordless.AspNetCore.Services;

public interface IPasswordlessService<TRegisterRequest>
{
    Task<IResult> RegisterUserAsync(TRegisterRequest request, CancellationToken cancellationToken);
    Task<IResult> LoginUserAsync(PasswordlessLoginRequest loginRequest, CancellationToken cancellationToken);
    Task<IResult> AddCredentialAsync(PasswordlessAddCredentialRequest request, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken);
}

public sealed class UserInformation
{
    public string Username { get; }
    public string? DisplayName { get; }
    public HashSet<string>? Aliases { get; }

    public UserInformation(string username, string? displayName, HashSet<string>? aliases)
    {
        Username = username;
        DisplayName = displayName;
        Aliases = aliases;
    }
}

public record PasswordlessAddCredentialRequest(string? DisplayName);
public record PasswordlessLoginRequest(string Token);

public class PasswordlessRegisterRequest
{
    public string Username { get; }
    public string? DisplayName { get; }
    public string? Email { get; set; }
    public HashSet<string>? Aliases { get; }

    public PasswordlessRegisterRequest(string username, string? displayName, HashSet<string>? aliases)
    {
        Username = username;
        DisplayName = displayName;
        Aliases = aliases;
    }
}
