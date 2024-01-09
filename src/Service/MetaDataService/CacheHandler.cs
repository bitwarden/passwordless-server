using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Passwordless.Service.MetaDataService;

public class CacheHandler : DelegatingHandler
{
    private readonly MetadataConfiguration _options;
    private readonly ILogger<CacheHandler> _logger;

    public CacheHandler(
        IOptions<MetadataConfiguration> options,
        ILogger<CacheHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            await File.CreateText($"{_options.Path}/mds.jwt").WriteAsync(content);
            _logger.LogInformation($"Updated MDS blob.");
        }
        else
        {
            var content = await File.ReadAllTextAsync($"{_options}/mds.jwt", cancellationToken);
            response.Content = new StringContent(content);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogWarning($"Using cached MDS blob.");
        }
        return response;
    }
}