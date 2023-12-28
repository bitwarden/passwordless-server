using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbGlobalSqliteContext : DbGlobalContext
{
    public DbGlobalSqliteContext(DbContextOptions<DbGlobalSqliteContext> options, TimeProvider timeProvider)
        : base(options, timeProvider)
    {
    }
}