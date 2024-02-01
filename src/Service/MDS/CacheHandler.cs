using System.Net;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MDS;

/// <summary>
/// The cache handler will attempt to download the latest MDS blob and cache it locally. The cache will be used
/// when the download fails. Use this if the intent is to automatically synchronize with the latest MDS blob from
/// the FIDO Alliance.
/// </summary>
public class CacheHandler : DelegatingHandler
{
    public const string Path = ".mds-cache/mds.jwt";

    private readonly ILogger<CacheHandler> _logger;

    public CacheHandler(ILogger<CacheHandler> logger)
    {
        _logger = logger;
    }

    public CacheHandler(HttpMessageHandler httpMessageHandler, ILogger<CacheHandler> logger) : base(httpMessageHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[CacheHandler] Attempting to synchronize with the latest blob from FIDO Alliance.");
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using (var cache = File.CreateText(Path))
            {
                await cache.WriteAsync(content);
            }
            _logger.LogInformation("[CacheHandler] Successfully downloaded the latest MDS blob.");
        }
        else
        {

            var content = await File.ReadAllTextAsync(Path, cancellationToken);
            response.Content = new StringContent(content);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogWarning("[CacheHandler] Downloading the latest MDS blob failed..");
        }
        return response;
    }
}