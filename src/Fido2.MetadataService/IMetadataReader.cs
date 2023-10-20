namespace Passwordless.Fido2.MetadataService;

public interface IMetadataReader
{
    Task<string> ReadAsync();
}