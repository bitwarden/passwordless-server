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
    DbTenantContext GetDbContext(string accountname);
}

public class MultiTenantSqliteDbTenantContextFactory : SqliteDbTenantContextFactory
{
    public MultiTenantSqliteDbTenantContextFactory(IOptions<SqliteTenantOptions> options) : base(options)
    {
    }

    protected override string GetDbPath(string tenant)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        // Create folder
        System.IO.Directory.CreateDirectory(System.IO.Path.Join(path, "passwordless"));

        return System.IO.Path.Join(path, "passwordless", $"multitenant.db");
    }
}

public class SqliteDbTenantContextFactory : IDbTenantContextFactory
{
    private IOptions<SqliteTenantOptions> _options;

    public SqliteDbTenantContextFactory(IOptions<SqliteTenantOptions> options)
    {
        _options = options;
    }

    protected virtual string GetDbPath(string tenant) {

        if (_options.Value.InMemory){
            return $"file:{tenant}?mode=memory&cache=shared";
        }

        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        // Create folder
        System.IO.Directory.CreateDirectory(System.IO.Path.Join(path, "passwordless"));

        return System.IO.Path.Join(path, "passwordless", $"{tenant}.db");
    }

    public async Task<IStorage> CreateNewTenant(string accountName)
    {
       
        var dbpath = GetDbPath(accountName);
        var options = new DbContextOptionsBuilder<DbTenantContext>()
            .UseSqlite($"Data Source={dbpath}")
            .Options;

        var dbContext = new DbTenantContext(options, new ManualTenantProvider(accountName));

        var exists = await dbContext.Database.CanConnectAsync();
        if(!_options.Value.InMemory && exists) {
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
            .UseSqlite($"Data Source={dbpath}")
            .Options;

        
        var dbContext = new DbTenantContext(options, new ManualTenantProvider(accountName));

        //// While dev
        //if (accountName != null)
        //{
        //    dbContext.Database.Migrate();
        //}

        return new EFStorage(dbContext);
    }

    public DbTenantContext GetDbContext(string accountName)
    {
        // TODO: Check if it exists
        var dbpath = GetDbPath(accountName);

        var options = new DbContextOptionsBuilder<DbTenantContext>()
            .UseSqlite($"Data Source={dbpath}")
            .Options;

        
        var dbContext = new DbTenantContext(options, new ManualTenantProvider(accountName));
        return dbContext; 
    }

    
}

public class SqliteTenantOptions {
    public string Path { get; set; }
    public bool InMemory { get; set; }
}
