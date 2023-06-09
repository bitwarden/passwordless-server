using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passwordless.AspNetCore;
using Passwordless.AspNetCore.Services;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder" /> to map Passwordless endpoints.
/// </summary>
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

        static async Task<IResult> PasswordlessRegister(
            TRegisterBody registerRequest,
            IPasswordlessService<TRegisterBody, TLoginBody, TAddCredentialBody> passwordlessService,
            CancellationToken cancellationToken)
        {
            return await passwordlessService.RegisterUserAsync(registerRequest, cancellationToken);
        }

        if (endpointOptions.RegisterPath is not null)
        {
            routeGroup.Map(endpointOptions.RegisterPath, PasswordlessRegister);
        }

        static async Task<IResult> PasswordlessLogin(
            TLoginBody loginRequest,
            IPasswordlessService<TRegisterBody, TLoginBody, TAddCredentialBody> passwordlessService,
            CancellationToken cancellationToken)
        {
            return await passwordlessService.LoginUserAsync(loginRequest, cancellationToken);
        }

        if (endpointOptions.LoginPath is not null)
        {
            routeGroup.MapPost(endpointOptions.LoginPath, PasswordlessLogin);
        }

        static async Task<IResult> PasswordlessAddCredential(
            TAddCredentialBody addCredentialRequest,
            IPasswordlessService<TRegisterBody, TLoginBody, TAddCredentialBody> passwordlessService,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken)
        {
            var userId = await passwordlessService.GetUserIdAsync(claimsPrincipal, cancellationToken);
            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            return await passwordlessService.AddCredentialAsync(userId, addCredentialRequest, cancellationToken);
        }

        if (endpointOptions.AddCredentialPath is not null)
        {
            routeGroup.MapPost(endpointOptions.AddCredentialPath, PasswordlessAddCredential);
            // Should we require authorization?
        }

        return routeGroup;
    }
}