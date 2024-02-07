using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MDS;

/// <summary>
/// The OfflineCacheHandler will attempt to read the MDS blob from the local file system. Use this if the intent is
/// to manually synchronize with the latest MDS blob from the FIDO Alliance. This is useful for self-hosted
/// deployments where there is no connectivity to the FIDO Alliance or internet.
/// </summary>
public sealed class OfflineCacheHandler : DelegatingHandler
{
    private const string Path = "mds.jwt";

    private readonly IConfiguration _configuration;
    private readonly ILogger<OfflineCacheHandler> _logger;

    public OfflineCacheHandler(
        IConfiguration configuration,
        ILogger<OfflineCacheHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        if (string.Equals(_configuration["fido2:mds:mode"], "Online", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("[OfflineCacheHandler] Synchronizing using the MDS blob with FIDO2 failed.");
            }
        }

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