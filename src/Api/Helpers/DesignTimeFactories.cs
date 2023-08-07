using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Helpers;

public class SqliteContextFactory : IDesignTimeDbContextFactory<DbGlobalSqliteContext>
{
    public DbGlobalSqliteContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DbGlobalSqliteContext>();
        const string devDefault = "Data Source=Data/passwordless_dev.db";
        options.UseSqlite(args.Length == 1 ? args[0] : devDefault);
        return new DbGlobalSqliteContext(options.Options);
    }
}

public class MsSqlContextFactory : IDesignTimeDbContextFactory<DbGlobalMsSqlContext>
{
    public DbGlobalMsSqlContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DbGlobalMsSqlContext>();
        options.UseSqlServer(args.Length == 1 ? args[0] : null);
        return new DbGlobalMsSqlContext(options.Options);
    }
}