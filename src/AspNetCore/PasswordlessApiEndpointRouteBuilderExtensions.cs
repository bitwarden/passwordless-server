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
    /// <summary>
    /// 
    /// </summary>
    public static PasswordlessEndpointConventionBuilder MapPasswordless(this IEndpointRouteBuilder endpoints)
    {
        // TODO: When a custom register body isn't passed in, we can make a reasonable assumption
        // about what each endpoint produces and we can build on those.
        var builder = endpoints.MapPasswordless(new PasswordlessEndpointOptions());
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    public static PasswordlessEndpointConventionBuilder MapPasswordless(this IEndpointRouteBuilder endpoints, PasswordlessEndpointOptions endpointOptions)
    {
        return endpoints.MapPasswordless<PasswordlessRegisterRequest>(endpointOptions);
    }

    /// <summary>
    /// 
    /// </summary>
    public static PasswordlessEndpointConventionBuilder MapPasswordless<TRegisterBody>(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPasswordless<PasswordlessRegisterRequest>(new PasswordlessEndpointOptions());
    }

    /// <summary>
    /// 
    /// </summary>
    public static PasswordlessEndpointConventionBuilder MapPasswordless<TRegisterBody>(this IEndpointRouteBuilder endpoints, PasswordlessEndpointOptions endpointOptions)
    {
        var routeGroup = endpoints
            .MapGroup(endpointOptions.GroupPrefix)
            .WithGroupName("Passwordless");

        static async Task<IResult> PasswordlessRegister(
            TRegisterBody registerRequest,
            IPasswordlessService<TRegisterBody> passwordlessService,
            CancellationToken cancellationToken)
        {
            return await passwordlessService.RegisterUserAsync(registerRequest, cancellationToken);
        }

        RouteHandlerBuilder? registerRouteHandler = null;
        if (endpointOptions.RegisterPath is not null)
        {
            registerRouteHandler = routeGroup.Map(endpointOptions.RegisterPath, PasswordlessRegister);
        }

        static async Task<IResult> PasswordlessLogin(
            PasswordlessLoginRequest loginRequest,
            IPasswordlessService<TRegisterBody> passwordlessService,
            CancellationToken cancellationToken)
        {
            return await passwordlessService.LoginUserAsync(loginRequest, cancellationToken);
        }

        RouteHandlerBuilder? loginRouteHandler = null;
        if (endpointOptions.LoginPath is not null)
        {
            loginRouteHandler = routeGroup.MapPost(endpointOptions.LoginPath, PasswordlessLogin);
        }

        static async Task<IResult> PasswordlessAddCredential(
            IPasswordlessService<TRegisterBody> passwordlessService,
            PasswordlessAddCredentialRequest request,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken)
        {
            return await passwordlessService.AddCredentialAsync(request, claimsPrincipal, cancellationToken);
        }

        RouteHandlerBuilder? addCredentialRouteHandler = null;
        if (endpointOptions.AddCredentialPath is not null)
        {
            addCredentialRouteHandler = routeGroup.MapPost(endpointOptions.AddCredentialPath, PasswordlessAddCredential);
            // Should we require authorization?
        }

        return new PasswordlessEndpointConventionBuilder(routeGroup,
            loginRouteHandler,
            registerRouteHandler,
            addCredentialRouteHandler);
    }
}

public sealed class PasswordlessEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly RouteGroupBuilder _groupBuilder;
    public RouteHandlerBuilder? LoginRoute { get; }
    public RouteHandlerBuilder? RegisterRoute { get; }
    public RouteHandlerBuilder? AddCredentialRoute { get; }

    public PasswordlessEndpointConventionBuilder(
        RouteGroupBuilder groupBuilder,
        RouteHandlerBuilder? loginRoute,
        RouteHandlerBuilder? registerRoute,
        RouteHandlerBuilder? addCredentialRoute)
    {
        _groupBuilder = groupBuilder;
        LoginRoute = loginRoute;
        RegisterRoute = registerRoute;
        AddCredentialRoute = addCredentialRoute;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        ((IEndpointConventionBuilder)_groupBuilder).Add(convention);
    }
}
