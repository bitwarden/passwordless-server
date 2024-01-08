using Fido2NetLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.MetaDataService;

public static class MetaDataServiceBootstrap
{
    public static void AddMetaDataService(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        builder.Services.Configure<MetadataConfiguration>(configuration.GetSection("fido2:mds"));
        
        // We will only use this for updating.
        builder.Services.AddTransient<CacheHandler>();
        builder.Services.AddHttpClient(nameof(Fido2MetadataServiceRepository), client =>
        {
            client.BaseAddress = new Uri("https://mds3.fidoalliance.org/");
        }).AddHttpMessageHandler<CacheHandler>();
        builder.Services.AddTransient<IMetadataRepository, Fido2MetadataServiceRepository>();
        //builder.Services.AddHostedService<MetaDataServiceUpdaterBackgroundService>();
        
        // The actual used service for attestation:
        //builder.Services.AddTransient<IMetadataRepository, FileSystemMetadataRepository>(_ => new FileSystemMetadataRepository(".mds-cache"));
        builder.Services.AddTransient<IMetadataService, DistributedCacheMetadataService>();
    }
}