using Passwordless.Common.Models.Reporting;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ReportingService : IReportingService
{

    private readonly ITenantStorage _tenantStorage;
    private readonly IGlobalStorage _storage;

    public ReportingService(
        ITenantStorage tenantStorage,
        IGlobalStorage storage)
    {
        _tenantStorage = tenantStorage;
        _storage = storage;
    }

    public Task<int> UpdatePeriodicCredentialReportsAsync()
    {
        return _storage.UpdatePeriodicCredentialReportsAsync();
    }

    public async Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest parameters)
    {
        var entities = await _tenantStorage.GetPeriodicCredentialReportsAsync(parameters.From, parameters.To);
        return entities
            .Select(x => new PeriodicCredentialReportResponse(x.CreatedAt, x.UsersCount, x.CredentialsCount));
    }

    public Task<int> UpdatePeriodicActiveUserReportsAsync()
    {
        return _storage.UpdatePeriodicActiveUserReportsAsync();
    }

    public async Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest parameters)
    {
        var entities = await _tenantStorage.GetPeriodicActiveUserReportsAsync(parameters.From, parameters.To);
        return entities
            .Select(x => new PeriodicActiveUserReportResponse(x.CreatedAt, x.DailyActiveUsersCount, x.WeeklyActiveUsersCount, x.TotalUsersCount));
    }
}