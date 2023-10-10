using Microsoft.EntityFrameworkCore;

namespace Passwordless.AdminConsole.Db;

public class MssqlConsoleDbContext : ConsoleDbContext
{
    private readonly IConfiguration _config;

    public MssqlConsoleDbContext(DbContextOptions<MssqlConsoleDbContext> options, IConfiguration config) :
        base(options)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(_config.GetConnectionString("mssql:admin"));
}

// public class MssqlContextFactory : IDesignTimeDbContextFactory<MssqlConsoleDbContext>
// {
//     public MssqlConsoleDbContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<MssqlConsoleDbContext>();
//         optionsBuilder.UseSqlServer(_config.GetConnectionString("mssql"));
//
//         return new MssqlConsoleDbContext(optionsBuilder.Options, null);
//     }
// }