using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public class AuthenticatorDataService<TDbContext> : IAuthenticatorDataService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    public AuthenticatorDataService(IDbContextFactory<TDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddOrUpdateAuthenticatorDataAsync(Guid aaGuid, string name)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var authenticator = await db.Authenticators.SingleOrDefaultAsync(x => x.AaGuid == aaGuid);
        if (authenticator == null)
        {
            authenticator = new Authenticator { AaGuid = aaGuid, Name = name };
            db.Authenticators.Add(authenticator);
        }
        else
        {
            authenticator.Name = name;
        }
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Authenticator>> GetAsync(IEnumerable<Guid> aaGuids)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Authenticators.Where(x => aaGuids.Contains(x.AaGuid)).ToListAsync();
    }
}