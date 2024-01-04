using Microsoft.EntityFrameworkCore;
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
                    CreatedAt = _timeProvider.GetUtcNow().UtcDateTime.Date
                });

        await _db.PeriodicCredentialReports.AddRangeAsync(result);

        var rows = await _db.SaveChangesAsync();

        return rows;
    }
}