using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Microsoft.AspNetCore.Routing;

public static class PasswordlessApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasswordless<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        var routeGroup = endpoints.MapGroup("");

        static async Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> PasswordlessRegister(
            PasswordlessRegisterRequest registerRequest,
            IPasswordlessClient passwordlessClient,
            IUserStore<TUser> userStore,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            var logger = loggerFactory.CreateLogger("PasswordlessRegister");

            var user = new TUser();
            await userStore.SetUserNameAsync(user, registerRequest.Username, cancellationToken);
            var result = await userStore.CreateAsync(user, cancellationToken);

            if (!result.Succeeded)
            {
                return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
            }

            var userId = await userStore.GetUserIdAsync(user, cancellationToken);
            logger.LogInformation("Created user with username {Username} and id {Id}", registerRequest.Username, userId);

            // Call passwordless
            var registerTokenResponse = await passwordlessClient.CreateRegisterToken(new RegisterOptions
            {
                UserId = userId,
                Username = registerRequest.Username,
                Aliases = registerRequest.Aliases,
            });

            return TypedResults.Ok(registerTokenResponse);
        }

        routeGroup.Map("/passwordless-register", PasswordlessRegister);

        static async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> PasswordlessLogin(
            PasswordlessLoginRequest loginRequest,
            IPasswordlessClient passwordlessClient,
            IUserStore<TUser> userStore,
            IUserClaimsPrincipalFactory<TUser> userClaimsPrincipalFactory,
            ILoggerFactory loggerFactory,
            HttpContext httpContext)
        {
            var logger = loggerFactory.CreateLogger("PasswordlessLogin");

            // Get token
            var verifiedUser = await passwordlessClient.VerifyToken(loginRequest.Token);

            if (verifiedUser is null)
            {
                logger.LogDebug("User could not be verified with token {Token}", loginRequest.Token);
                return TypedResults.Unauthorized();
            }

            logger.LogDebug("Attempting to find user in store by id {UserId}.", verifiedUser.UserId);
            var user = await userStore.FindByIdAsync(verifiedUser.UserId, httpContext.RequestAborted);

            if (user is null)
            {
                logger.LogDebug("Could not find user.");
                return TypedResults.Unauthorized();
            }

            var claimsPrincipal = await userClaimsPrincipalFactory.CreateAsync(user);

            // TODO: Not totally sure what the best scheme is
            var authenticationOptions = httpContext.RequestServices.GetService<IOptions<AuthenticationOptions>>()?.Value;

            var scheme = authenticationOptions?.DefaultSignInScheme;

            logger.LogInformation("Signing in user with scheme {Scheme} and {NumberOfClaims} claims",
                scheme, claimsPrincipal.Claims.Count());

            return TypedResults.SignIn(claimsPrincipal, authenticationScheme: scheme);
        }

        routeGroup.MapPost("/passwordless-login", PasswordlessLogin);

        return routeGroup;
    }
}

record PasswordlessRegisterRequest(string Username, string? DisplayName, HashSet<string>? Aliases);

record PasswordlessLoginRequest(string Token);
