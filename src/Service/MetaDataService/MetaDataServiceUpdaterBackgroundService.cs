using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MetaDataService;

/// <summary>
/// Runs a background service that periodically updates the MDS blob.
/// </summary>
public sealed class MetaDataServiceUpdaterBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetaDataServiceUpdaterBackgroundService> _logger;

    public MetaDataServiceUpdaterBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MetaDataServiceUpdaterBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(MetaDataServiceUpdaterBackgroundService)} is running.");
        using PeriodicTimer timer = new(TimeSpan.FromDays(7));
        try
        {
            await DoWorkAsync();
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{nameof(MetaDataServiceUpdaterBackgroundService)} is stopping.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{nameof(MetaDataServiceUpdaterBackgroundService)} failed.");
        }
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var remoteRepository = scope.ServiceProvider.GetRequiredService<Fido2MetadataServiceRepository>();
            var blob = await remoteRepository.GetBLOBAsync();
            foreach (var entry in blob.Entries)
            {
                var serializedMetadataStatement = JsonSerializer.Serialize(entry.MetadataStatement, FidoModelSerializerContext.Default.MetadataStatement);
                File.CreateText($".mds-cache/{entry.AaGuid}.json").Write(serializedMetadataStatement);
            }
            _logger.LogInformation("Updated MDS blob.");
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to update MDS blob: {error}", e.Message);
        }
    }
}