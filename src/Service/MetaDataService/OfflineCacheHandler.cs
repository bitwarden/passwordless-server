using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MetaDataService;

/// <summary>
/// For self-hosting, we might not have a way to update the MDS blob.
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
        if (!File.Exists(Path))
        {
            _logger.LogError("No offline MDS blob found at '{Path}'.", Path);
        }
        var content = await File.ReadAllTextAsync(Path, cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(content)
        };

        return response;
    }
}