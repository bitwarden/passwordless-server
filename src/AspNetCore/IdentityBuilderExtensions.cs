using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Passwordless.AspNetCore;
using Passwordless.Net;

namespace Microsoft.Extensions.DependencyInjection;

public class PasswordlessAspNetCoreOptions
{
    public PasswordlessOptions Api { get; set; } = new PasswordlessOptions();
    public Func<ConvertUserContext, Task> ConvertUser { get; set; } = DefaultConvertUser;


    private static Task DefaultConvertUser(ConvertUserContext convertUserContext)
    {
        return Task.CompletedTask;
    }
}

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddPasswordless(this IdentityBuilder builder, Action<PasswordlessAspNetCoreOptions>? configureOptions = null)
    {
        builder.Services.AddProblemDetails();

        builder.Services.AddOptions<PasswordlessAspNetCoreOptions>()
            .Configure(o => configureOptions?.Invoke(o))
            .PostConfigure<IConfiguration>((options, config) =>
            {
                var passwordlessSection = config.GetSection("Passwordless");

                if (passwordlessSection == null)
                {
                    return;
                }

                if (options.Api.ApiSecret == null)
                {
                    options.Api.ApiSecret = passwordlessSection.GetValue<string>("Api:ApiSecret")
                        ?? passwordlessSection.GetValue<string>("ApiSecret")!;
                }

                var configApiUrl = passwordlessSection.GetValue<string>("Api:ApiUrl")
                        ?? passwordlessSection.GetValue<string>("ApiUrl");

                if (configApiUrl != null)
                {
                    options.Api.ApiUrl = configApiUrl;
                }
            });

        builder.Services.AddPasswordlessSdk(passwordlessOptions => { });

        builder.Services.AddOptions<PasswordlessOptions>()
            .Configure<IOptions<PasswordlessAspNetCoreOptions>>((options, aspNetCoreOptionsAccessor) =>
            {
                var aspNetCoreOptions = aspNetCoreOptionsAccessor.Value;
                options.ApiSecret = aspNetCoreOptions.Api.ApiSecret;
                options.ApiUrl = aspNetCoreOptions.Api.ApiUrl;
            });

        builder.Services.AddScoped(
            typeof(IPasswordlessIdentityService),
            typeof(PasswordlessIdentityService<>).MakeGenericType(builder.UserType)
        );

        return builder;
    }
}
