using Microsoft.EntityFrameworkCore;

namespace Passwordless.AdminConsole.Helpers;

public static class DbContextHelpers
{

    public static async Task<bool> HaveAnyMigrationsEverBeenAppliedAsync(this DbContext context) =>
        (await context.Database.GetAppliedMigrationsAsync()).Any();
}