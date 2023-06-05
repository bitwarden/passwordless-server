using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Passwordless.AspNetCore;
using Passwordless.AspNetCore.Services;
using Passwordless.Net;

namespace Microsoft.AspNetCore.Routing;

public static class PasswordlessApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasswordless(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPasswordless(new PasswordlessEndpointOptions());
    }

    public static IEndpointConventionBuilder MapPasswordless(this IEndpointRouteBuilder endpoints, PasswordlessEndpointOptions endpointOptions)
    {
        return endpoints.MapPasswordless<PasswordlessRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>(endpointOptions);
    }

    // TODO: Add documentation about how customization of the bodies means you must also customize that service
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static IEndpointConventionBuilder MapPasswordless<TRegisterBody, TLoginBody, TAddCredentialBody>(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPasswordless<PasswordlessRegisterRequest, PasswordlessLoginRequest, PasswordlessAddCredentialRequest>(new PasswordlessEndpointOptions());
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static IEndpointConventionBuilder MapPasswordless<TRegisterBody, TLoginBody, TAddCredentialBody>(this IEndpointRouteBuilder endpoints, PasswordlessEndpointOptions endpointOptions)
    {
        var routeGroup = endpoints
            .MapGroup(endpointOptions.GroupPrefix)
            .WithGroupName("Passwordless");


        static async Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> PasswordlessRegister(
            TRegisterBody registerRequest,
            IRegisterService<TRegisterBody> registerService,
            CancellationToken cancellationToken)
        {
            return await registerService.RegisterAsync(registerRequest, cancellationToken);
        }

        if (endpointOptions.RegisterPath is not null)
        {
            routeGroup.Map(endpointOptions.RegisterPath, PasswordlessRegister)
                .WithName("Register");
        }

        static async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> PasswordlessLogin(
            TLoginBody loginRequest,
            ILoginService<TLoginBody> loginService,
            CancellationToken cancellationToken)
        {
            return await loginService.LoginAsync(loginRequest, cancellationToken);
        }

        if (endpointOptions.LoginPath is not null)
        {
            routeGroup.MapPost(endpointOptions.LoginPath, PasswordlessLogin)
                .WithName("Login");
        }

        static async Task<Results<Ok<RegisterTokenResponse>, UnauthorizedHttpResult>> PasswordlessAddCredential(
            TAddCredentialBody addCredentialRequest,
            IAddCredentialService<TAddCredentialBody> addCredentialService,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken)
        {
            return await addCredentialService.AddCredentialAsync(addCredentialRequest, claimsPrincipal, cancellationToken);
        }

        if (endpointOptions.AddCredentialPath is not null)
        {
            routeGroup.MapPost(endpointOptions.AddCredentialPath, PasswordlessAddCredential)
                .WithName("AddCredential");
            // Should we require authorization?
        }

        return routeGroup;
    }
}
