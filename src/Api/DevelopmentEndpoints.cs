using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
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

            // seed with a development api key
            if (!await dbContext.ApiKeys.AnyAsync())
            {
                dbContext.ApiKeys.Add(new ApiKeyDesc()
                {
                    Tenant = "test",
                    Id = "9795",
                    ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795"
                });
                dbContext.ApiKeys.Add(new ApiKeyDesc()
                {
                    Tenant = "test",
                    Id = "6d02",
                    ApiKey = "4RtmMr0hVknaQAIhaRtPHw==:xR7bg3NVsC80a8GDDhH39g==",
                    Scopes = new[] { "token_register", "token_verify" },
                });
                dbContext.AccountInfo.Add(new AccountMetaInformation() { Tenant = "test", AcountName = "test" });
                await dbContext.SaveChangesAsync();

                return "Database created and seeded.";
            }

            return "All OK. Happy Developing!";
        });
        
        app.MapGet("health/throw/api", (ctx) => throw new ApiException("test_error", "Testing error response", 400));
        app.MapGet("health/throw/exception", (ctx) => throw new Exception("Testing error response", new Exception("Inner exception")));
    }
}