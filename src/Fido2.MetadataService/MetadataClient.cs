using Microsoft.Extensions.Logging;
using Passwordless.Fido2.MetadataService.Exceptions;

namespace Passwordless.Fido2.MetadataService;

public sealed class MetadataClient : IMetadataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetadataClient> _logger;

    public MetadataClient(
        HttpClient httpClient,
        ILogger<MetadataClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <summary>
    /// Downloads the latest MDS3 JWT blob.
    /// </summary>
    /// <returns></returns>
    public async Task<string> DownloadAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(string.Empty);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new Fido2Exception("Failed to obtain latest Fido2 metadata.", response.StatusCode);

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to obtain latest Fido2 metadata: {Message}", ex.Message);
            throw new Fido2Exception("Failed to obtain latest Fido2 metadata.", ex.StatusCode);
        }
    }
}