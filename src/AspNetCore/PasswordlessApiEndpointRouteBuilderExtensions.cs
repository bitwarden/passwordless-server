using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Passwordless.AspNetCore;
using Passwordless.Net;

namespace Microsoft.AspNetCore.Routing;

public static class PasswordlessApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasswordless<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        var routeGroup = endpoints.MapGroup("");

        routeGroup.MapPost("/passwordless-register", async Task<Results<Ok<RegisterTokenResponse>, BadRequest>>
            (HttpContext httpContext, IServiceProvider services, IPasswordlessClient passwordlessClient, IPasswordlessService<TUser> passwordlessService, IUserStore<TUser> userStore) =>
        {
            // Get user id
            var user = await passwordlessService.GetUserAsync(httpContext);

            if (user is null)
            {
                // TODO: Make better erro
                return TypedResults.BadRequest();
            }

            var userId = await userStore.GetUserIdAsync(user, httpContext.RequestAborted);
            var username = await userStore.GetUserNameAsync(user, httpContext.RequestAborted);

            // Call passwordless

            var registerOptions = new RegisterOptions
            {
                UserId = userId,
                Username = username ?? "",
                DisplayName = username ?? httpContext.User.Identity?.Name,
                AliasHashing = false, // FROM OPTIONS
                Discoverable = true, // FROM OPTIONS
            };

            if (username != null)
            {
                registerOptions.Aliases = new HashSet<string> { username };
            }

            var registerTokenResponse = await passwordlessClient.CreateRegisterToken(registerOptions);

            return TypedResults.Ok(registerTokenResponse);
        });

        routeGroup.MapPost("/passwordless-login", async Task<Results<UnauthorizedHttpResult, Ok, SignInHttpResult>>
            (HttpContext httpContext,
                PasswordlessLoginRequest loginRequest,
                IPasswordlessClient passwordlessClient,
                IOptions<AuthenticationOptions> authenticationOptionsAccessor,
                IUserStore<TUser> userStore,
                IUserClaimsPrincipalFactory<TUser> claimsFactory) =>
        {
            // Get token
            var verifiedUser = await passwordlessClient.VerifyToken(loginRequest.Token);

            if (verifiedUser is null)
            {
                return TypedResults.Unauthorized();
            }

            var user = await userStore.FindByIdAsync(verifiedUser.UserId, httpContext.RequestAborted);

            if (user is null)
            {
                return TypedResults.Unauthorized();
            }

            var claimsPrincipal = await claimsFactory.CreateAsync(user);

            // TODO: Not totally sure what the best scheme is
            var authenticationOptions = authenticationOptionsAccessor.Value;

            return TypedResults.SignIn(claimsPrincipal, authenticationScheme: authenticationOptions.DefaultSignInScheme);
        });

        return routeGroup;
    }
}

record PasswordlessRegisterRequest(string Username, string? DisplayName, HashSet<string>? Aliases);

record PasswordlessLoginRequest(string Token);
