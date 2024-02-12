using Passwordless.Common.Models.Reporting;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ReportingService : IReportingService
{

    private readonly ITenantStorage _tenantStorage;
    private readonly IGlobalStorageFactory _storageFactory;

    public ReportingService(
        ITenantStorage tenantStorage,
        IGlobalStorageFactory storageFactory)
    {
        _tenantStorage = tenantStorage;
        _storageFactory = storageFactory;
    }

    public Task<int> UpdatePeriodicCredentialReportsAsync()
    {
        var storage = _storageFactory.Create();
        return storage.UpdatePeriodicCredentialReportsAsync();
    }

    public async Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest parameters)
    {
        var entities = await _tenantStorage.GetPeriodicCredentialReportsAsync(parameters.From, parameters.To);
        return entities
            .Select(x => new PeriodicCredentialReportResponse(x.CreatedAt, x.UsersCount, x.CredentialsCount));
    }

    public Task<int> UpdatePeriodicActiveUserReportsAsync()
    {
        var storage = _storageFactory.Create();
        return storage.UpdatePeriodicActiveUserReportsAsync();
    }

    public async Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest parameters)
    {
        var storage = _tenantStorageFactory.Create();
        var entities = await storage.GetPeriodicActiveUserReportsAsync(parameters.From, parameters.To);
        return entities
            .Select(x => new PeriodicActiveUserReportResponse(x.CreatedAt, x.DailyActiveUsersCount, x.WeeklyActiveUsersCount, x.TotalUsersCount));
    }
}