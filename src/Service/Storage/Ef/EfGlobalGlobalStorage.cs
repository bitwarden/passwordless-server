using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class EfGlobalGlobalStorage : IGlobalStorage
{

    private readonly DbGlobalContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EfGlobalGlobalStorage> _logger;

    public EfGlobalGlobalStorage(
        DbGlobalContext db,
        TimeProvider timeProvider,
        ILogger<EfGlobalGlobalStorage> logger)
    {
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
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
        try
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

            return await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogWarning(e, "Concurrency exception occurred while updating PeriodicCredentialReports.");
            return 0;
        }
    }

    /// <summary>
    /// Records the amount of daily or weekly active users per tenant.
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdatePeriodicActiveUserReportsAsync()
    {
        try
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

            await _db.PeriodicActiveUserReports.AddRangeAsync(result);

            return await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            _logger.LogWarning(e, "Concurrency exception occurred while updating PeriodicActiveUserReports.");
            return 0;
        }
    }

    public async Task DeleteOldDispatchedEmailsAsync(TimeSpan age)
    {
        var until = _timeProvider.GetUtcNow().UtcDateTime - age;
        await _db.DispatchedEmails
            .Where(x => x.CreatedAt < until)
            .ExecuteDeleteAsync();

        await _db.SaveChangesAsync();
    }
}