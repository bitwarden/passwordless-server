using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MDS;

/// <summary>
/// The OfflineCacheHandler will attempt to read the MDS blob from the local file system. Use this if the intent is
/// to manually synchronize with the latest MDS blob from the FIDO Alliance. This is useful for self-hosted
/// deployments where there is no connectivity to the FIDO Alliance or internet.
/// </summary>
public sealed class OfflineCacheHandler : DelegatingHandler
{
    private const string Path = "/etc/bitwarden_passwordless/mds.jwt";

    private readonly ILogger<OfflineCacheHandler> _logger;

    public OfflineCacheHandler(ILogger<OfflineCacheHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[OfflineCacheHandler] Synchronizing using the MDS blob found at '{Path}'.", Path);
        if (!File.Exists(Path))
        {
            _logger.LogError("[OfflineCacheHandler] No offline MDS blob found at '{Path}'.", Path);
        }
        var content = await File.ReadAllTextAsync(Path, cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(content)
        };

        return response;
    }
}