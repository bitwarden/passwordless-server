using Microsoft.EntityFrameworkCore;

namespace Passwordless.AdminConsole.Db;

public class MssqlConsoleDbContext : ConsoleDbContext
{
    public MssqlConsoleDbContext(DbContextOptions<MssqlConsoleDbContext> options) : base(options)
    {
    }
}