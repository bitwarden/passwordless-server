using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AspNetCore.Services;
using Passwordless.Net;

namespace Microsoft.Extensions.DependencyInjection;

public class PasswordlessAspNetCoreOptions : PasswordlessOptions
{

}

// TODO: Add documentation
public static class IdentityBuilderExtensions
{
    public static IServiceCollection AddPasswordless<TUser>(this IServiceCollection services, IConfiguration configuration)
        where TUser : class, new()
    {
        return services.AddPasswordless<TUser>(configuration.Bind);
    }

    public static IServiceCollection AddPasswordless<TUser>(this IServiceCollection services, Action<PasswordlessAspNetCoreOptions> configureOptions)
        where TUser : class, new()
    {
        return services.AddPasswordlessCore(typeof(TUser), configureOptions);
    }

    public static IServiceCollection AddPasswordless(this IdentityBuilder identityBuilder, IConfiguration configuration)
    {
        return identityBuilder.Services.AddPasswordlessCore(identityBuilder.UserType, configuration.Bind);
    }

    public static IServiceCollection AddPasswordless(this IdentityBuilder identityBuilder, Action<PasswordlessAspNetCoreOptions> configureOptions)
    {
        return identityBuilder.Services.AddPasswordlessCore(identityBuilder.UserType, configureOptions);
    }

    private static IServiceCollection AddPasswordlessCore(this IServiceCollection services, Type userType, Action<PasswordlessAspNetCoreOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddPasswordlessSdk(passwordlessOptions => { });

        services.TryAddScoped(typeof(IRegisterService<PasswordlessRegisterRequest>), typeof(RegisterService<>).MakeGenericType(userType));
        services.TryAddScoped(typeof(ILoginService<PasswordlessLoginRequest>), typeof(LoginService<>).MakeGenericType(userType));
        services.TryAddScoped(typeof(IAddCredentialService<PasswordlessAddCredentialRequest>), typeof(AddCredentialService<>).MakeGenericType(userType));

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
