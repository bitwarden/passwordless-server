using Microsoft.EntityFrameworkCore;
using Passwordless.Common.Utils;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class EfGlobalGlobalStorage : IGlobalStorage
{

    private readonly DbGlobalContext _db;
    private readonly TimeProvider _timeProvider;

    public EfGlobalGlobalStorage(DbGlobalContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<ICollection<string>> GetApplicationsPendingDeletionAsync()
    {
        // Will replace IgnoreQueryFilters at a later stage. To possibly use different db contexts.
        var tenants = await _db.AccountInfo
            .Where(x => x.DeleteAt <= _timeProvider.GetUtcNow().UtcDateTime)
            .Select(x => x.Tenant)
            .ToListAsync();
        return tenants;
    }

    /// <summary>
    /// Returns the amount of credentials and unique users per tenant.
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdatePeriodicCredentialReportsAsync()
    {
        var result = _db.AccountInfo
            .GroupJoin(
                _db.Credentials,
                accountInfo => accountInfo.Tenant,
                credential => credential.Tenant,
                (accountInformation, credentials) => new PeriodicCredentialReport
                {
                    Tenant = accountInformation.Tenant,
                    UsersCount = credentials.Select(x => x.UserId).Distinct().Count(),
                    CredentialsCount = credentials.Count(),
                    CreatedAt = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime)
                });

        _db.PeriodicCredentialReports.AddRange(result);

        var rows = await _db.SaveChangesAsync();

        return rows;
    }

    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        var appId = ApiKeyUtils.GetAppId(apiKey);
        var pk = apiKey.Substring(apiKey.Length - 4);
        return _db.ApiKeys.FirstOrDefaultAsync(e => e.Id == pk && e.Tenant == appId);
    }

    /// <summary>
    /// Records the amount of daily or weekly active users per tenant.
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdatePeriodicActiveUserReportsAsync()
    {
        var result = _db.AccountInfo
            .GroupJoin(
                _db.Credentials,
                accountInfo => accountInfo.Tenant,
                credential => credential.Tenant,
                (accountInformation, credentials) => new PeriodicActiveUserReport
                {
                    Tenant = accountInformation.Tenant,

                    DailyActiveUsersCount = credentials
                        .Where(x => x.LastUsedAt >= _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1))
                        .Select(x => x.UserId)
                        .Distinct()
                        .Count(),

                    WeeklyActiveUsersCount = credentials
                        .Where(x => x.LastUsedAt >= _timeProvider.GetUtcNow().UtcDateTime.AddDays(-7))
                        .Select(x => x.UserId)
                        .Distinct()
                        .Count(),

                    TotalUsersCount = credentials
                        .Select(x => x.UserId)
                        .Distinct()
                        .Count(),

                    CreatedAt = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime)
                });

        _db.PeriodicActiveUserReports.AddRange(result);

        return await _db.SaveChangesAsync();
    }
}