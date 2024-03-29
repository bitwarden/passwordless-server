using Fido2NetLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.MDS;

public static class MetaDataServiceBootstrap
{
    public static void AddMetaDataService(this WebApplicationBuilder builder)
    {
        var httpClientBuilder = builder.Services.AddHttpClient(nameof(Fido2MetadataServiceRepository), client =>
        {
            client.BaseAddress = new Uri("https://mds3.fidoalliance.org/");
        });

        builder.Services.AddTransient<CacheHandler>();
        httpClientBuilder.AddHttpMessageHandler<CacheHandler>();

        builder.Services.AddSingleton<IMetadataRepository, Fido2MetadataServiceRepository>();
        builder.Services.AddSingleton<IMetadataService, DistributedCacheMetadataService>();
    }
}