using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Fido2.MetadataService;

public static class MetadataServiceBootstrap
{
    public static void AddMetadataService(this IServiceCollection services)
    {
        services.AddHttpClient<IMetadataClient, MetadataClient>(client =>
        {
            client.BaseAddress = new Uri("https://mds3.fidoalliance.org");
        });
        services.AddSingleton<IJwtTokenReader, JwtTokenReader>();
        services.AddSingleton<IMetadataReader, MetadataReader>();
    }
}