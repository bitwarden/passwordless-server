using Fido2NetLib;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Models.MDS;

namespace Passwordless.Service.MDS;

public sealed class MetaDataService : DistributedCacheMetadataService, IMetaDataService
{
    public MetaDataService(
        IEnumerable<IMetadataRepository> repositories,
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<DistributedCacheMetadataService> logger, ISystemClock systemClock)
        : base(
            repositories,
            distributedCache,
            memoryCache,
            logger,
            systemClock)
    {
    }

    public async Task<IEnumerable<string>> GetAttestationTypesAsync()
    {
        var blob = await GetDistributedCachedBlob(base._repositories.First());
        var result = blob.Entries
            .Where(x => x.MetadataStatement.ProtocolFamily == "fido2")
            .SelectMany(x => x.MetadataStatement.AttestationTypes)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return result;
    }

    public async Task<IEnumerable<EntryResponse>> GetEntriesAsync(EntriesRequest request)
    {
        var blob = await GetDistributedCachedBlob(base._repositories.First());
        var result = blob.Entries
            .Where(x => x.MetadataStatement.ProtocolFamily == "fido2")
            .OrderBy(x => x.MetadataStatement.Description)
            .Select(x =>
                new EntryResponse(
                    x.AaGuid!.Value,
                    x.MetadataStatement.Description,
                    x.StatusReports.Select(statusReport => statusReport.Status.ToString()),
                    x.MetadataStatement.AttestationTypes)
            );

        if (request.AttestationTypes != null && request.AttestationTypes.Any())
        {
            result = result.Where(x => x.AttestationTypes.Any(y => request.AttestationTypes.Contains(y)));
        }

        if (request.CertificationStatuses != null && request.CertificationStatuses.Any())
        {
            result = result.Where(x => x.CertificationStatuses.Any(y => request.CertificationStatuses.Contains(y)));
        }

        return result.ToList();
    }

    public async Task<IEnumerable<string>> GetCertificationStatusesAsync()
    {
        var blob = await GetDistributedCachedBlob(base._repositories.First());
        var result = blob.Entries
            .Where(x => x.MetadataStatement.ProtocolFamily == "fido2")
            .SelectMany(x => x.StatusReports)
            .Select(x => x.Status)
            .Distinct()
            .Select(x => x.ToString())
            .OrderBy(x => x)
            .ToList();
        return result;
    }
}