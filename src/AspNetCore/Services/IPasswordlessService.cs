using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AspNetCore.Services;

public interface IPasswordlessService<TRegisterRequest, TLoginRequest, TAddCredentialRequest>
{
    Task<IResult> RegisterUserAsync(TRegisterRequest request, CancellationToken cancellationToken);
    Task<IResult> LoginUserAsync(TLoginRequest loginRequest, CancellationToken cancellationToken);
    Task<IResult> AddCredentialAsync(string userId, TAddCredentialRequest request, CancellationToken cancellationToken);
    ValueTask<string?> GetUserIdAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken);
}

public class PasswordlessService<TUser> : PasswordlessService<TUser, PasswordlessRegisterRequest>
    where TUser : class, new()
{
    public PasswordlessService(
        IPasswordlessClient passwordlessClient,
        IUserStore<TUser> userStore,
        ILogger<PasswordlessService<TUser, PasswordlessRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>> logger,
        IOptions<PasswordlessAspNetCoreOptions> optionsAccessor,
        IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
        IServiceProvider serviceProvider)
        : base(passwordlessClient, userStore, logger, optionsAccessor, userClaimsPrincipalFactory, serviceProvider)
    {
    }
}

public class PasswordlessService<TUser, TRegisterRequest> : PasswordlessService<TUser, TRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>
    where TUser : class, new()
    where TRegisterRequest : PasswordlessRegisterRequest
{
    public PasswordlessService(
        IPasswordlessClient passwordlessClient,
        IUserStore<TUser> userStore,
        ILogger<PasswordlessService<TUser, TRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>> logger,
        IOptions<PasswordlessAspNetCoreOptions> optionsAccessor,
        IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
        IServiceProvider serviceProvider)
        : base(passwordlessClient, userStore, logger, optionsAccessor, userClaimsPrincipalFactory, serviceProvider)
    {
    }
}

public class PasswordlessService<TUser, TRegisterRequest, TLoginRequest, TAddCredentialRequest>
    : IPasswordlessService<TRegisterRequest, TLoginRequest, TAddCredentialRequest>
    where TUser : class, new()
    where TRegisterRequest : PasswordlessRegisterRequest, TAddCredentialRequest
    where TLoginRequest : PasswordlessLoginRequest
    where TAddCredentialRequest : PasswordlessAddCredentialRequest
{
    private readonly ILogger<PasswordlessService<TUser, TRegisterRequest, TLoginRequest, TAddCredentialRequest>> _logger;

    public PasswordlessService(
        IPasswordlessClient passwordlessClient,
        IUserStore<TUser> userStore,
        ILogger<PasswordlessService<TUser, TRegisterRequest, TLoginRequest, TAddCredentialRequest>> logger,
        IOptions<PasswordlessAspNetCoreOptions> optionsAccessor,
        IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
        IServiceProvider serviceProvider)
    {
        PasswordlessClient = passwordlessClient;
        UserStore = userStore;
        _logger = logger;
        UserClaimsPrincipalFactory = userClaimsPrincipalFactory;
        Options = optionsAccessor.Value;
        IdentityOptions = serviceProvider.GetService<IOptions<IdentityOptions>>()?.Value;
        AuthenticationOptions = serviceProvider.GetService<IOptions<AuthenticationOptions>>()?.Value;
    }

    protected IPasswordlessClient PasswordlessClient { get; }
    protected IUserStore<TUser> UserStore { get; }
    protected IUserClaimsPrincipalFactory<TUser> UserClaimsPrincipalFactory { get; }
    protected PasswordlessAspNetCoreOptions Options { get; }
    protected IdentityOptions? IdentityOptions { get; }
    protected AuthenticationOptions? AuthenticationOptions { get; }

    public virtual async Task<IResult> AddCredentialAsync(string userId, TAddCredentialRequest request, CancellationToken cancellationToken)
    {
        var user = await UserStore.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        return await AddCredentialAsync(user, request, cancellationToken);
    }

    protected virtual async Task<IResult> AddCredentialAsync(TUser user, TAddCredentialRequest request, CancellationToken cancellationToken)
    {
        var userId = await UserStore.GetUserIdAsync(user, cancellationToken);
        var username = await UserStore.GetUserNameAsync(user, cancellationToken);
        var registerOptions = CreateRegisterOptions(userId, displayName: null, username!, request.Aliases);

        // Allow registeroptions customization
        var registerTokenResponse = await PasswordlessClient.CreateRegisterToken(registerOptions);
        return TypedResults.Ok(registerTokenResponse);
    }

    public virtual ValueTask<string?> GetUserIdAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        if (claimsPrincipal.Identity?.IsAuthenticated is not true)
        {
            return ValueTask.FromResult<string?>(null);
        }

        // First try our own options, fallback to built in Identity options
        // and then fallback to ClaimsIdentity default
        var userIdClaim = Options.UserIdClaimType
            ?? IdentityOptions?.ClaimsIdentity.UserIdClaimType
            ?? ClaimTypes.NameIdentifier;

        var userId = claimsPrincipal.FindFirstValue(userIdClaim);

        return ValueTask.FromResult(userId);
    }

    public virtual async Task<IResult> RegisterUserAsync(TRegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new TUser();
        await BindToUserAsync(user, request, cancellationToken);
        var result = await UserStore.CreateAsync(user, cancellationToken);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })); ;
        }

        return await AddCredentialAsync(user, request, cancellationToken);
    }

    public virtual async Task<IResult> LoginUserAsync(TLoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var verifiedUser = await PasswordlessClient.VerifyToken(loginRequest.Token);

        if (verifiedUser is null)
        {
            _logger.LogDebug("User could not be verified with token {Token}", loginRequest.Token);
            return TypedResults.Unauthorized();
        }

        _logger.LogDebug("Attempting to find user in store by id {UserId}.", verifiedUser.UserId);
        var user = await UserStore.FindByIdAsync(verifiedUser.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogDebug("Could not find user.");
            return TypedResults.Unauthorized();
        }

        var claimsPrincipal = await UserClaimsPrincipalFactory.CreateAsync(user);

        // First try our own scheme, then optionally try built in options but null is still allowed because it
        // will then fallback to the default scheme.
        var scheme = Options.SignInScheme
            ?? AuthenticationOptions?.DefaultSignInScheme;

        _logger.LogInformation("Signing in user with scheme {Scheme} and {NumberOfClaims} claims",
            scheme, claimsPrincipal.Claims.Count());

        return TypedResults.SignIn(claimsPrincipal, authenticationScheme: scheme);
    }

    protected virtual async Task BindToUserAsync(TUser user, TRegisterRequest request, CancellationToken cancellationToken)
    {
        await UserStore.SetUserNameAsync(user, request.Username, cancellationToken);
    }

    protected virtual RegisterOptions CreateRegisterOptions(
        string userId,
        string? displayName,
        string username,
        HashSet<string>? aliases)
    {
        return new RegisterOptions
        {
            UserId = userId,
            DisplayName = displayName,
            Username = username,
            Aliases = aliases,
            Discoverable = Options.Discoverable,
            UserVerification = Options.UserVerification,
            Attestation = Options.Attestation,
            // TODO: Is this used?
            // ExpiresAt = Options.Expiration.HasValue ? DateTime.UtcNow.Add(Options.Expiration.Value) : null,
            AuthenticatorType = Options.AuthenticationType!, // TODO: Does register options have the best annotations?
        };
    }
}

public record PasswordlessAddCredentialRequest(HashSet<string>? Aliases);
public record PasswordlessLoginRequest(string Token);
public record PasswordlessRegisterRequest(string Username, string? DisplayName, HashSet<string>? Aliases) : PasswordlessAddCredentialRequest(Aliases);