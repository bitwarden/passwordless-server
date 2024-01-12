// ReSharper disable UnusedType.Global

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Helpers;

/// <summary>
/// Do not delete this file. It is used by EF Core to create migrations.
/// </summary>
public class SqliteContextFactory : IDesignTimeDbContextFactory<DbGlobalSqliteContext>
{
    private readonly TimeProvider _timeProvider;

    public SqliteContextFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DbGlobalSqliteContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DbGlobalSqliteContext>();
        const string devDefault = "Data Source=Data/passwordless_dev.db";
        options.UseSqlite(args.Length == 1 ? args[0] : devDefault);
        return new DbGlobalSqliteContext(options.Options, _timeProvider);
    }
}

/// <summary>
/// Do not delete this file. It is used by EF Core to create migrations.
/// </summary>
public class MsSqlContextFactory : IDesignTimeDbContextFactory<DbGlobalMsSqlContext>
{
    private readonly TimeProvider _timeProvider;

    public MsSqlContextFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DbGlobalMsSqlContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DbGlobalMsSqlContext>();
        options.UseSqlServer(args.Length == 1 ? args[0] : null);
        return new DbGlobalMsSqlContext(options.Options, _timeProvider);
    }
}