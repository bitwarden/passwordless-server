#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Service;
public class TestService
{
    private MyDataContext db;
    private readonly ILogger logger;

    public TestService(MyDataContext db, ILogger<TestService> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task<Student> AddStudent()
    {

        var student = new Student()
        {
            Name = "Anders",
            Email = "anders@passwordless.dev"
        };

        var sw = Stopwatch.StartNew();
        await db.Database.MigrateAsync();
        sw.Stop();

        
        logger.LogInformation("Migration took {duration}", sw.ElapsedMilliseconds);

        sw.Restart();
        db.Students.Add(student);
        await db.SaveChangesAsync();
        sw.Stop();
        logger.LogInformation("Insert took {duration}", sw.Elapsed);

        return student;
    }
}

public class Student
{
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class MyDataContext : DbContext
{
    public string DbPath { get; }

    public MyDataContext(
        DbContextOptions<MyDataContext> options,
        ITenantProvider tenantProvider
        ) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        // tenant
        var tenant = tenantProvider.Tenant;
        // Create folder
        System.IO.Directory.CreateDirectory(System.IO.Path.Join(path, "passwordless"));
        DbPath = System.IO.Path.Join(path, "passwordless", $"{tenant}.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");


    public DbSet<Student> Students => Set<Student>();
}

public class MyDataContextFactory : IDesignTimeDbContextFactory<MyDataContext>
{
    private ITenantProvider provider;

    public MyDataContextFactory(ITenantProvider provider) : base()
    {
        this.provider = provider;
    }

    public MyDataContext CreateDbContext(string[] args)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        // tenant
        var tenant = this.provider.Tenant;
        var DbPath = System.IO.Path.Join(path, "passwordless", $"{tenant}.db");

        var optionsBuilder = new DbContextOptionsBuilder<MyDataContext>();
        optionsBuilder.UseSqlite($"Data Source={DbPath}");

        return new MyDataContext(optionsBuilder.Options, this.provider);
    }
}