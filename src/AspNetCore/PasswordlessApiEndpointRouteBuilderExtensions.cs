using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Microsoft.AspNetCore.Routing;

public static class PasswordlessApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasswordless<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        var routeGroup = endpoints.MapGroup("");

        routeGroup.Map("/passwordless-register", async Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>>
            (IServiceProvider services, PasswordlessRegisterRequest registerRequest, IPasswordlessClient passwordlessClient) =>
        {
            var userManager = services.GetRequiredService<UserManager<TUser>>();

            var user = new TUser();
            await userManager.SetUserNameAsync(user, registerRequest.Username);
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
            }

            var userId = await userManager.GetUserIdAsync(user);

            // Call passwordless
            var registerTokenResponse = await passwordlessClient.CreateRegisterToken(new RegisterOptions
            {
                UserId = userId,
                Username = registerRequest.Username,
                Aliases = registerRequest.Aliases,
            });

            return TypedResults.Ok(registerTokenResponse);
        });

        routeGroup.MapPost("/passwordless-login", async Task<Results<UnauthorizedHttpResult, Ok, SignInHttpResult>>
            (IServiceProvider services, PasswordlessLoginRequest loginRequest, IPasswordlessClient passwordlessClient, IOptions<AuthenticationOptions> authenticationOptionsAccessor) =>
        {
            // Get token
            var verifiedUser = await passwordlessClient.VerifyToken(loginRequest.Token);

            if (verifiedUser is null)
            {
                return TypedResults.Unauthorized();
            }

            var userManager = services.GetRequiredService<UserManager<TUser>>();

            var user = await userManager.FindByIdAsync(verifiedUser.UserId);

            if (user is null)
            {
                return TypedResults.Unauthorized();
            }

            var claimsFactory = services.GetRequiredService<IUserClaimsPrincipalFactory<TUser>>();
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
