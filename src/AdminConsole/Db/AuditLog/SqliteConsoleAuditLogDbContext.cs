using Microsoft.EntityFrameworkCore;

namespace AdminConsole.Db.AuditLog;

public class SqliteConsoleAuditLogDbContext : ConsoleAuditLogDbContext
{
    private readonly IConfiguration _config;

    public SqliteConsoleAuditLogDbContext(DbContextOptions<SqliteConsoleAuditLogDbContext> options, IConfiguration config) : base(options)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(_config.GetConnectionString("sqlite:audit"));
}