using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbGlobalMsSqlContext : DbGlobalContext
{
    public DbGlobalMsSqlContext(DbContextOptions<DbGlobalMsSqlContext> options, TimeProvider timeProvider)
        : base(options, timeProvider)
    {
    }
}