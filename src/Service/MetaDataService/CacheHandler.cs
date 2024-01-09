using System.Net;
using Microsoft.Extensions.Logging;

namespace Passwordless.Service.MetaDataService;

public class CacheHandler : DelegatingHandler
{
    private const string Path = ".mds-cache/mds.jwt";

    private readonly ILogger<CacheHandler> _logger;

    public CacheHandler(ILogger<CacheHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            await File.CreateText(Path).WriteAsync(content);
            _logger.LogInformation($"Updated MDS blob.");
        }
        else
        {
            var content = await File.ReadAllTextAsync(Path, cancellationToken);
            response.Content = new StringContent(content);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogWarning($"Using cached MDS blob.");
        }
        return response;
    }
}