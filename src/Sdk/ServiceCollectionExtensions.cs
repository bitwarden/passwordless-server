using Microsoft.Extensions.Options;
using Passwordless.Net;

// This is a trick to always show up in a class when people are registering services
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordlessSdk(this IServiceCollection services, Action<PasswordlessOptions> configureOptions)
    {
        services.AddOptions<PasswordlessOptions>()
            .Configure(configureOptions)
            .PostConfigure(options => options.ApiUrl ??= PasswordlessOptions.CloudApiUrl)
            .Validate(options => !string.IsNullOrEmpty(options.ApiSecret), "Passwordless: Missing ApiSecret");

        services.AddPasswordlessHttpClient();

        // TODO: Get rid of this service, all consumers should use the interface
        services.AddTransient(sp => (PasswordlessClient)sp.GetRequiredService<IPasswordlessClient>());

        return services;
    }

    private static void AddPasswordlessHttpClient(this IServiceCollection services)
    {
        services.AddSingleton<PasswordlessDelegatingHandler>();

        services.AddHttpClient<IPasswordlessClient, PasswordlessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PasswordlessOptions>>().Value;

            client.BaseAddress = new Uri(options.ApiUrl);
            client.DefaultRequestHeaders.Add("ApiSecret", options.ApiSecret);
        })
            .AddHttpMessageHandler<PasswordlessDelegatingHandler>();
    }
}