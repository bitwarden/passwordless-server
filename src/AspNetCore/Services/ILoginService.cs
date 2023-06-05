using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AspNetCore.Services;

public interface ILoginService<TBody>
{
    Task<Results<SignInHttpResult, UnauthorizedHttpResult>> LoginAsync(TBody body, CancellationToken cancellationToken);
}

// Public so that others can use it as a base class if they want to customize only a little
public class LoginService<TUser> : ILoginService<PasswordlessLoginRequest>
    where TUser : class, new()
{
    private readonly IOptions<AuthenticationOptions>? _authenticationOptions;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IUserStore<TUser> _userStore;
    private readonly IUserClaimsPrincipalFactory<TUser> _userClaimsPrincipalFactory;
    private readonly ILogger<LoginService<TUser>> _logger;

    public LoginService(
        IServiceProvider serviceProvider,
        IPasswordlessClient passwordlessClient,
        IUserStore<TUser> userStore,
        IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
        ILogger<LoginService<TUser>> logger)
    {
        _authenticationOptions = serviceProvider.GetService<IOptions<AuthenticationOptions>>();
        _passwordlessClient = passwordlessClient;
        _userStore = userStore;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _logger = logger;
    }

    public async virtual Task<Results<SignInHttpResult, UnauthorizedHttpResult>> LoginAsync(PasswordlessLoginRequest request, CancellationToken cancellationToken)
    {
        // Get token
        var verifiedUser = await _passwordlessClient.VerifyToken(request.Token);

        if (verifiedUser is null)
        {
            _logger.LogDebug("User could not be verified with token {Token}", request.Token);
            return TypedResults.Unauthorized();
        }

        _logger.LogDebug("Attempting to find user in store by id {UserId}.", verifiedUser.UserId);
        var user = await _userStore.FindByIdAsync(verifiedUser.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogDebug("Could not find user.");
            return TypedResults.Unauthorized();
        }

        var claimsPrincipal = await _userClaimsPrincipalFactory.CreateAsync(user);

        // TODO: Not totally sure what the best scheme is
        var scheme = _authenticationOptions?.Value?.DefaultSignInScheme;

        _logger.LogInformation("Signing in user with scheme {Scheme} and {NumberOfClaims} claims",
            scheme, claimsPrincipal.Claims.Count());

        return TypedResults.SignIn(claimsPrincipal, authenticationScheme: scheme);
    }
}

public record PasswordlessLoginRequest(string Token);
