using Microsoft.EntityFrameworkCore;

namespace AdminConsole.Db;

public class SqliteConsoleDbContext : ConsoleDbContext
{
    private readonly IConfiguration _config;

    public SqliteConsoleDbContext(DbContextOptions<SqliteConsoleDbContext> options, IConfiguration config) :
        base(options)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(_config.GetConnectionString("sqlite:admin"));
}