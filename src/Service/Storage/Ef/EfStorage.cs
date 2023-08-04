using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class EfStorage : IStorage
{

    private readonly DbTenantContext _db;
    private readonly ISystemClock _systemClock;

    public EfStorage(DbTenantContext db, ISystemClock systemClock)
    {
        _db = db;
        _systemClock = systemClock;
    }

    public async Task<ICollection<string>> GetApplicationsPendingDeletionAsync()
    {
        // Will replace IgnoreQueryFilters at a later stage. To possibly use different db contexts.
        var tenants = await _db.AccountInfo.IgnoreQueryFilters()
            .Where(x => x.DeleteAt <= _systemClock.UtcNow.UtcDateTime)
            .Select(x => x.Tenant)
            .ToListAsync();
        return tenants;
    }
}