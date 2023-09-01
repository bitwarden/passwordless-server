using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api;

public static class DevelopmentEndpoints
{
    public static void UseDevelopmentEndpoints(this WebApplication app)
    {
        app.Map("/", async (DbGlobalContext dbContext) =>
        {
            // TODO: If people complain, put this behind ?do=migrate and just return a link here.
            await dbContext.Database.MigrateAsync();

            if (await dbContext.ApiKeys.AnyAsync())
            {
                return "All OK. Happy Developing!";
            }

            // seed with a development api key
            await dbContext.SeedDefaultApplicationAsync(
                "test",
                "test:public:2e728aa5986f4ba8b073a5b28a939795",
                "test:secret:a679563b331846c79c20b114a4f56d02");
            await dbContext.SaveChangesAsync();
            return "Database created and seeded.";
        });
    }
}