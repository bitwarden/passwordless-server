using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;

namespace Passwordless.AdminConsole.Tests.Factory;

public static class DbContextFactory
{
    public static ConsoleDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ConsoleDbContext>()
            .UseInMemoryDatabase(databaseName: $"admin-{Guid.NewGuid():N}")
            .Options;
        return Create(options);
    }

    public static ConsoleDbContext Create(DbContextOptions<ConsoleDbContext> options)
    {
        return new ConsoleDbContext(options);
    }
}