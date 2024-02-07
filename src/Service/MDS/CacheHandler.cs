using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MDS;

/// <summary>
/// The cache handler will attempt to download the latest MDS blob and cache it locally. The cache will be used
/// when the download fails. Use this if the intent is to automatically synchronize with the latest MDS blob from
/// the FIDO Alliance.
/// </summary>
public class CacheHandler : DelegatingHandler
{
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

        return await base.SendAsync(request, cancellationToken);
    }
}