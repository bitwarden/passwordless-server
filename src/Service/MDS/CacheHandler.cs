using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MDS;

/// <summary>
/// First attempt to retrieve the MDS blob from FIDO2, when it fails, use the local file system.
/// </summary>
public sealed class CacheHandler : DelegatingHandler
{
    public const string Path = "mds.jwt";

    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheHandler> _logger;

    public CacheHandler(
        IConfiguration configuration,
        ILogger<CacheHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public CacheHandler(
        HttpMessageHandler httpMessageHandler,
        IConfiguration configuration,
        ILogger<CacheHandler> logger) : base(httpMessageHandler)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        // If we're running in online mode, move to the next handler, or execute the request if this is the last handler.
        if (string.Equals(_configuration["fido2:mds:mode"], "Online", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException)
            {
                // Request failed, can happen when domain name cannot be resolved, etc.
                _logger.LogInformation("[OfflineCacheHandler] Synchronizing using the MDS blob with FIDO2 failed.");
            }
        }

        // If the response has failed, we need to attempt to read the MDS blob from the local file system.
        if (response == null || !response.IsSuccessStatusCode)
        {
            _logger.LogInformation("[OfflineCacheHandler] Synchronizing using the MDS blob found at '{Path}'.", Path);
            if (!File.Exists(Path))
            {
                _logger.LogError("[OfflineCacheHandler] No offline MDS blob found at '{Path}'.", Path);
            }

            var jwt = await File.ReadAllTextAsync(Path, cancellationToken);
            var content = new StringContent(jwt);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content,
            };
        }

        return response;
    }
}