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

        services.AddPasswordlessClientCore<IPasswordlessClient, PasswordlessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PasswordlessOptions>>().Value;

            client.BaseAddress = new Uri(options.ApiUrl);
            client.DefaultRequestHeaders.Add("ApiSecret", options.ApiSecret);
        });

        // TODO: Get rid of this service, all consumers should use the interface
        services.AddTransient(sp => (PasswordlessClient)sp.GetRequiredService<IPasswordlessClient>());

        return services;
    }

    /// <summary>
    /// Helper method for making custom typed HttpClient implementations that also have
    /// the inner handler for throwing fancy exceptions. Not intended for public use,
    /// hence the hiding of it in IDE's.
    /// </summary>
    /// <remarks>
    /// This method signature is subject to change without major version bump/announcement.
    /// </remarks>
    internal static IServiceCollection AddPasswordlessClientCore<TClient, TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddTransient<PasswordlessDelegatingHandler>();

        services
            .AddHttpClient<TClient, TImplementation>(configureClient)
            .AddHttpMessageHandler<PasswordlessDelegatingHandler>();

        return services;
    }
}
