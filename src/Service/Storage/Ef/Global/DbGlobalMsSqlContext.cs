using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef.Global;

public class DbGlobalMsSqlContext : DbGlobalContext
{
    public DbGlobalMsSqlContext(DbContextOptions<DbGlobalMsSqlContext> options)
        : base(options)
    {
    }
}