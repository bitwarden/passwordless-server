// ReSharper disable UnusedType.Global

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Passwordless.AdminConsole.Db;

/// <summary>
/// Do not delete this file. It is used by EF Core to create migrations.
/// </summary>
public class MsSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MssqlConsoleDbContext>
{
    public MssqlConsoleDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MssqlConsoleDbContext>();
        options.UseSqlServer(args.Length == 1 ? args[0] : null);
        return new MssqlConsoleDbContext(options.Options);
    }
}