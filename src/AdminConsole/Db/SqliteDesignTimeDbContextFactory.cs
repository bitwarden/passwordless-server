// ReSharper disable UnusedType.Global

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Passwordless.AdminConsole.Db;

/// <summary>
/// Do not delete this file. It is used by EF Core to create migrations.
/// </summary>
public class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteConsoleDbContext>
{
    public SqliteConsoleDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteConsoleDbContext>();
        const string devDefault = "Data Source=adminconsole_dev.db";
        options.UseSqlite(args.Length == 1 ? args[0] : devDefault);
        return new SqliteConsoleDbContext(options.Options);
    }
}