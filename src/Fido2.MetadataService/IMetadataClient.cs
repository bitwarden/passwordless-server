namespace Passwordless.Fido2.MetadataService;

public interface IMetadataClient
{
    Task<string> DownloadAsync();
}