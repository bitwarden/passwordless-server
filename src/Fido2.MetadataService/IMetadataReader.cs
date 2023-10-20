using Passwordless.Fido2.MetadataService.Models;

namespace Passwordless.Fido2.MetadataService;

public interface IMetadataReader
{
    Task<IReadOnlyCollection<Authenticator>> ReadAsync();
}