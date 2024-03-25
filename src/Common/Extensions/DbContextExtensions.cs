using Microsoft.EntityFrameworkCore;

namespace Passwordless.Common.Extensions;

public static class DbContextExtensions
{
    public static bool HasAppliedMigrations(this DbContext context) =>
        context.Database.GetAppliedMigrations().Any();

    public static async Task<bool> HasAppliedMigrationsAsync(this DbContext context) =>
        (await context.Database.GetAppliedMigrationsAsync()).Any();
}