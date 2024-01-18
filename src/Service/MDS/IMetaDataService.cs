using Passwordless.Common.Models.MDS;

namespace Passwordless.Service.MDS;

public interface IMetaDataService
{
    Task<IEnumerable<string>> GetAttestationTypesAsync();
    Task<IEnumerable<AuthenticatorDto>> GetEntriesAsync();
    Task<IEnumerable<string>> GetCertificationStatusesAsync();
}