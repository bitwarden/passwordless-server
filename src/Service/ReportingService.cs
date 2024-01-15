using Passwordless.Common.Models.Reporting;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ReportingService : IReportingService
{

    private readonly ITenantStorageFactory _tenantStorageFactory;
    private readonly IGlobalStorageFactory _storageFactory;

    public ReportingService(
        ITenantStorageFactory tenantStorageFactory,
        IGlobalStorageFactory storageFactory)
    {
        _tenantStorageFactory = tenantStorageFactory;
        _storageFactory = storageFactory;
    }

    public Task<int> UpdatePeriodicCredentialReportsAsync()
    {
        var storage = _storageFactory.Create();
        return storage.UpdatePeriodicCredentialReportsAsync();
    }

    public async Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest parameters)
    {
        var storage = _tenantStorageFactory.Create();
        var entities = await storage.GetPeriodicCredentialReportsAsync(parameters.From, parameters.To);
        return entities
            .Select(x => new PeriodicCredentialReportResponse(x.CreatedAt, x.UsersCount, x.CredentialsCount))
            .ToList();
    }
}