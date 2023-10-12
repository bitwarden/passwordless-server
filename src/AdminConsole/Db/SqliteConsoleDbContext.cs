using Microsoft.EntityFrameworkCore;

namespace Passwordless.AdminConsole.Db;

public class SqliteConsoleDbContext : ConsoleDbContext
{
    public SqliteConsoleDbContext(DbContextOptions<SqliteConsoleDbContext> options) : base(options)
    {
    }
}