using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Service.Storage;

public interface IDbTenantContextFactory
{
    Task<IStorage> CreateNewTenant(string accountName);
    IStorage GetExistingTenant(string accountName);
}

public class SqliteDbTenantContextFactory : IDbTenantContextFactory
{
    public SqliteDbTenantContextFactory(IOptions<SqliteTenantOptions> options)
    {
        
    }

    private string GetDbPath(string tenant) {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        // tenant

        // Create folder
        System.IO.Directory.CreateDirectory(System.IO.Path.Join(path, "passwordless"));

        return System.IO.Path.Join(path, "passwordless", $"{tenant}.db");

    }

    public async Task<IStorage> CreateNewTenant(string accountName)
    {
       
        var dbpath = GetDbPath(accountName);
        var options = new DbContextOptionsBuilder<DbTenantContext>()
            .UseSqlite($"DataSource={dbpath}")
            .Options;

        var dbContext = new DbTenantContext(options, new ManualTenantProvider(accountName));

        var exists = await dbContext.Database.CanConnectAsync();
        if(exists) {
            throw new ArgumentException("Exists");
        }

        await dbContext.Database.MigrateAsync();

        //dbContext.Database.GetAppliedMigrations();

        return new EFStorage(dbContext);
    }

    public IStorage GetExistingTenant(string accountName)
    {
        if(accountName == null) {
            return new NoOpStorage();
        }
        // TODO: Check if it exists
        var dbpath = GetDbPath(accountName);

        var options = new DbContextOptionsBuilder<DbTenantContext>()
            .UseSqlite($"Datasource={dbpath}")
            .Options;

        
        var dbContext = new DbTenantContext(options, new ManualTenantProvider(accountName));

        // While dev
        if(accountName != null) {
            dbContext.Database.Migrate();
        }

        return new EFStorage(dbContext);
    }

    
}

public class SqliteTenantOptions {
    public string Path { get; set; }
}
