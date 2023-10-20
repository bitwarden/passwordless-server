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
    
    public async Task<string> ReadAsync()
    {
        var jwt = await _metadataClient.DownloadAsync();
        var json = _jwtTokenReader.Read(jwt);
        return json;
    }
}