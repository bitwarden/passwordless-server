using Passwordless.Common.Models.MDS;

namespace Passwordless.Service.MDS;

public interface IMetaDataService
{
    Task<IEnumerable<string>> GetAttestationTypesAsync();
    Task<IEnumerable<EntryResponse>> GetEntriesAsync(EntriesRequest request);
    Task<IEnumerable<string>> GetCertificationStatusesAsync();

    /// <summary>
    /// Checks whether all the given authenticators exist in the FIDO2 MDS.
    /// </summary>
    /// <param name="aaGuids"></param>
    /// <returns>Returns `true` if all the authenticators exist in the FIDO2 MDS; otherwise, `false`.</returns>
    Task<bool> ExistsAsync(IReadOnlyCollection<Guid> aaGuids);
}