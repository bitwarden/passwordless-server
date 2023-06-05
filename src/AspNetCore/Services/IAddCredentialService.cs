using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AspNetCore.Services;

public interface IAddCredentialService<TBody>
{
    Task<Results<Ok<RegisterTokenResponse>, UnauthorizedHttpResult>> AddCredentialAsync(
        TBody request,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken);
}

public class AddCredentialService<TUser> : IAddCredentialService<PasswordlessAddCredentialRequest>
    where TUser : class
{
    private readonly IUserStore<TUser> _userStore;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IOptions<IdentityOptions>? _identityOptions;

    public AddCredentialService(IUserStore<TUser> userStore, IPasswordlessClient passwordlessClient, IServiceProvider serviceProvider)
    {
        _userStore = userStore;
        _passwordlessClient = passwordlessClient;
        _identityOptions = serviceProvider.GetService<IOptions<IdentityOptions>>();
    }

    public async Task<Results<Ok<RegisterTokenResponse>, UnauthorizedHttpResult>> AddCredentialAsync(
        PasswordlessAddCredentialRequest request,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        if (claimsPrincipal.Identity?.IsAuthenticated is not true)
        {
            return TypedResults.Unauthorized();
        }

        // TODO: Could allow this to be customized but our own options
        var userIdClaim = _identityOptions?.Value.ClaimsIdentity.UserIdClaimType
            ?? ClaimTypes.NameIdentifier;

        var userId = claimsPrincipal.FindFirstValue(userIdClaim);

        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Unauthorized();
        }

        var user = await _userStore.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var username = await _userStore.GetUserNameAsync(user, cancellationToken);

        if (string.IsNullOrEmpty(username))
        {
            return TypedResults.Unauthorized();
        }

        var response = await _passwordlessClient.CreateRegisterToken(new RegisterOptions
        {
            UserId = userId,
            Username = username,
        });

        return TypedResults.Ok(response);
    }
}

public record PasswordlessAddCredentialRequest(HashSet<string>? Aliases);
