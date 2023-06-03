using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Microsoft.Extensions.DependencyInjection;

public class PasswordlessAspNetCoreOptions : PasswordlessOptions
{

}

public static class IdentityBuilderExtensions
{
    public static IServiceCollection AddPasswordless(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPasswordless(configuration.Bind);
    }

    public static IServiceCollection AddPasswordless(this IServiceCollection services, Action<PasswordlessAspNetCoreOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddPasswordlessSdk(passwordlessOptions => { });

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
