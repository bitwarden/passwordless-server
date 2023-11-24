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

    public async Task AddOrUpdateAuthenticatorDataAsync(Guid aaGuid, string name, string icon)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var authenticator = await db.Authenticators.SingleOrDefaultAsync(x => x.AaGuid == aaGuid);
        if (authenticator == null)
        {
            authenticator = new Authenticator { AaGuid = aaGuid, Name = name, Icon = icon };
            db.Authenticators.Add(authenticator);
        }
        else
        {
            authenticator.Name = name;
            authenticator.Icon = icon;
        }
        await db.SaveChangesAsync();
    }
}