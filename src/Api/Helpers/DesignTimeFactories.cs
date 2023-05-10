using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Helpers;

public class SqliteContextFactory : IDesignTimeDbContextFactory<SqliteContext>
{
    public SqliteContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteContext>();
        options.UseSqlite(args.Length == 1 ? args[0] : null);
        return new SqliteContext(options.Options, new ManualTenantProvider(null!));
    }
}

public class MsSqlContextFactory : IDesignTimeDbContextFactory<MsSqlContext>
{
    public MsSqlContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MsSqlContext>();
        options.UseSqlServer(args.Length == 1 ? args[0] : null);
        return new MsSqlContext(options.Options, new ManualTenantProvider(null!));
    }
}