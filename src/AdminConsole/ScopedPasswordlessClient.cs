using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AdminConsole;

public static class ScopedPasswordlessClient
{
    public static IServiceCollection AddScopedPasswordlessSdk(this IServiceCollection services)
    {
        services.AddOptions<PasswordlessOptions>()
            .PostConfigure(options => options.ApiUrl ??= PasswordlessOptions.CloudApiUrl)
            .Validate(options => !string.IsNullOrEmpty(options.ApiSecret), "Passwordless: Missing ApiSecret");
        
        services.AddTransient<ScopedApiSecretDelegatingHandler>();
        services.AddSingleton<IScopedPasswordlessContext, ScopedPasswordlessContext>();

        services.AddPasswordlessClientCore((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PasswordlessOptions>>().Value;

                client.BaseAddress = new Uri(options.ApiUrl);
            })
            .AddHttpMessageHandler<ScopedApiSecretDelegatingHandler>()
            .AddHttpMessageHandler<PasswordlessDelegatingHandler>();
    
        return services;
    }
}