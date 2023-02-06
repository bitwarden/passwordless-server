using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DbTenantContextDesignTimeFactory : IDesignTimeDbContextFactory<DbTenantContext>
{
    public DbTenantContext CreateDbContext(string[] args)
    {     

        var optionsBuilder = new DbContextOptionsBuilder<DbTenantContext>();
        optionsBuilder.UseSqlite("Data Source=_designtime.db");

        return new DbTenantContext(optionsBuilder.Options, new ManualTenantProvider(""));
    }
}