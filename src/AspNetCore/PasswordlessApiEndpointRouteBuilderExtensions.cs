using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Passwordless.AspNetCore.Services;
using Passwordless.Net;

namespace Microsoft.AspNetCore.Routing;

public static class PasswordlessApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasswordless(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPasswordless<PasswordlessRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>();
    }

    public static IEndpointConventionBuilder MapPasswordless<TRegisterBody, TLoginBody, TAddCredentialBody>(this IEndpointRouteBuilder endpoints)
    {
        var routeGroup = endpoints.MapGroup("");

        static async Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> PasswordlessRegister(
            TRegisterBody registerRequest,
            IRegisterService<TRegisterBody> registerService,
            CancellationToken cancellationToken)
        {
            return await registerService.RegisterAsync(registerRequest, cancellationToken);
        }

        routeGroup.Map("/passwordless-register", PasswordlessRegister);

        static async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> PasswordlessLogin(
            TLoginBody loginRequest,
            ILoginService<TLoginBody> loginService,
            CancellationToken cancellationToken)
        {
            return await loginService.LoginAsync(loginRequest, cancellationToken);
        }

        routeGroup.MapPost("/passwordless-login", PasswordlessLogin);

        static async Task<Results<Ok<RegisterTokenResponse>, UnauthorizedHttpResult>> PasswordlessAddCredential(
            TAddCredentialBody addCredentialRequest,
            IAddCredentialService<TAddCredentialBody> addCredentialService,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken)
        {
            return await addCredentialService.AddCredentialAsync(addCredentialRequest, claimsPrincipal, cancellationToken);
        }

        routeGroup.MapPost("/passwordless-add-credential", PasswordlessAddCredential);
        // Should we require authorization?

        return routeGroup;
    }
}
