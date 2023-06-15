using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AspNetCore;
using Passwordless.AspNetCore.Services;
using Passwordless.AspNetCore.Services.Implementations;
using Passwordless.Net;

// Trick to make it show up where it's more likely to be useful
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Default extensions to <see cref="IServiceCollection"/> and <see cref="IdentityBuilder"/> for <see cref="PasswordlessApiEndpointRouteBuilderExtensions.MapPasswordless(IEndpointRouteBuilder)"/>.
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    /// Adds the services to support <see cref="PasswordlessApiEndpointRouteBuilderExtensions.MapPasswordless(IEndpointRouteBuilder)" />
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <param name="configuration">The <see cref="IConfiguration" /> to use to bind to <see cref="PasswordlessAspNetCoreOptions" />. Generally it's own section.</param>
    /// <returns>The <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddPasswordless<TUser>(this IServiceCollection services, IConfiguration configuration)
        where TUser : class, new()
    {
        return services.AddPasswordless<TUser>(configuration.Bind);
    }

    /// <summary>
    /// Adds the services to support <see cref="PasswordlessApiEndpointRouteBuilderExtensions.MapPasswordless(IEndpointRouteBuilder)" />
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <param name="configure">Configures the <see cref="PasswordlessAspNetCoreOptions" />.</param>
    /// <returns>The <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddPasswordless<TUser>(this IServiceCollection services, Action<PasswordlessAspNetCoreOptions> configure)
        where TUser : class, new()
    {
        return services.AddPasswordlessCore(typeof(TUser), configure);
    }


    /// <summary>
    /// Adds the services to support <see cref="PasswordlessApiEndpointRouteBuilderExtensions.MapPasswordless(IEndpointRouteBuilder)" />
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder" /> instance.</param>
    /// <param name="configuration">The <see cref="IConfiguration" /> to use to bind to <see cref="PasswordlessAspNetCoreOptions" />. Generally it's own section.</param>
    /// <returns>The <see cref="IdentityBuilder" />.</returns>
    public static IdentityBuilder AddPasswordless(this IdentityBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddPasswordlessCore(builder.UserType, configuration.Bind);
        return builder;
    }

    /// <summary>
    /// Adds the services to support <see cref="PasswordlessApiEndpointRouteBuilderExtensions.MapPasswordless(IEndpointRouteBuilder)" />
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder" /> instance.</param>
    /// <param name="configure">Configures the <see cref="PasswordlessAspNetCoreOptions" />.</param>
    /// <returns>The <see cref="IdentityBuilder" />.</returns>
    public static IdentityBuilder AddPasswordless(this IdentityBuilder builder, Action<PasswordlessAspNetCoreOptions> configure)
    {
        builder.Services.AddPasswordlessCore(builder.UserType, configure);
        return builder;
    }

    private static IServiceCollection AddPasswordlessCore(this IServiceCollection services,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type userType,
        Action<PasswordlessAspNetCoreOptions> configure)
    {
        services.Configure(configure);

        // Add the SDK services but don't configure it there since ASP.NET Core options are a superset of their options.
        services.AddPasswordlessSdk(passwordlessOptions => { });

        services.TryAddScoped(
            typeof(IPasswordlessService<PasswordlessRegisterRequest>),
            typeof(PasswordlessService<>).MakeGenericType(userType));

        services.TryAddScoped<ICustomizeRegisterOptions, NoopCustomizeRegisterOptions>();

        // Override SDK options to come from ASP.NET Core options
        services.AddOptions<PasswordlessOptions>()
            .Configure<IOptions<PasswordlessAspNetCoreOptions>>((options, aspNetCoreOptionsAccessor) =>
            {
                var aspNetCoreOptions = aspNetCoreOptionsAccessor.Value;
                options.ApiSecret = aspNetCoreOptions.ApiSecret;
                options.ApiUrl = aspNetCoreOptions.ApiUrl;
            });

        return services;
    }
}