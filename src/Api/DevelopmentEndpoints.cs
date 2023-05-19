using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

public static class DevelopmentEndpoints
{
    public static void UseDevelopmentEndpoints(this WebApplication app)
    {
        app.Map("/", async (DbTenantContext dbTenantContext) =>
        {
            // TODO: If people complain, put this behind ?do=migrate and just return a link here.
            await dbTenantContext.Database.MigrateAsync();

            // seed with a development api key
            if (!await dbTenantContext.ApiKeys.AnyAsync())
            {
                dbTenantContext.ApiKeys.Add(new ApiKeyDesc()
                {
                    Tenant = "test",
                    Id = "9795",
                    ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795"
                });
                dbTenantContext.ApiKeys.Add(new ApiKeyDesc()
                {
                    Tenant = "test",
                    Id = "6d02",
                    ApiKey = "4RtmMr0hVknaQAIhaRtPHw==:xR7bg3NVsC80a8GDDhH39g==",
                    Scopes = new[] { "token_register", "token_verify" },
                });
                dbTenantContext.AccountInfo.Add(new AccountMetaInformation() { Tenant = "test", AcountName = "test" });
                await dbTenantContext.SaveChangesAsync();

                return "Database created and seeded.";
            }

            return "All OK. Happy Developing!";
        });
    }
}