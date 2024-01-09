using Fido2NetLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.MetaDataService;

public static class MetaDataServiceBootstrap
{
    public static void AddMetaDataService(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var httpClientBuilder = builder.Services.AddHttpClient(nameof(Fido2MetadataServiceRepository), client =>
        {
            client.BaseAddress = new Uri("https://mds3.fidoalliance.org/");
        });

        if (configuration["SelfHosted"] == "true" && configuration["fido2:mds:mode"] != "Online")
        {
            builder.Services.AddTransient<OfflineCacheHandler>();
            httpClientBuilder.AddHttpMessageHandler<OfflineCacheHandler>();
        }
        else
        {
            builder.Services.AddTransient<CacheHandler>();
            httpClientBuilder.AddHttpMessageHandler<CacheHandler>();
        }

        builder.Services.AddTransient<IMetadataRepository, Fido2MetadataServiceRepository>();
        builder.Services.AddTransient<IMetadataService, DistributedCacheMetadataService>();
    }
}