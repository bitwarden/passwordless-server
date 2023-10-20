using System.Text.Json;
using Passwordless.Fido2.MetadataService.Dtos;
using Passwordless.Fido2.MetadataService.Models;

namespace Passwordless.Fido2.MetadataService;

public class MetadataReader : IMetadataReader
{
    private readonly IMetadataClient _metadataClient;
    private readonly IJwtTokenReader _jwtTokenReader;
    
    public MetadataReader(
        IMetadataClient metadataClient,
        IJwtTokenReader jwtTokenReader)
    {
        _metadataClient = metadataClient;
        _jwtTokenReader = jwtTokenReader;
    }
    
    public async Task<IReadOnlyCollection<Authenticator>> ReadAsync()
    {
        var jwt = await _metadataClient.DownloadAsync();
        var json = _jwtTokenReader.Read(jwt);
        var dto = JsonSerializer.Deserialize<MetadataMessageDto>(json);
        return dto.Entries
            .Select(x => new Authenticator
            {
                AaGuid = x.AaGuid,
                Description = x.Statement.Description
            })
            .ToList();
    }
}