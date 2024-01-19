using Passwordless.Common.Models.MDS;

namespace Passwordless.Service.MDS;

public interface IMetaDataService
{
    Task<IEnumerable<string>> GetAttestationTypesAsync();
    Task<IEnumerable<EntryResponse>> GetEntriesAsync(EntriesRequest request);
    Task<IEnumerable<string>> GetCertificationStatusesAsync();
}