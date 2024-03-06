using Microsoft.EntityFrameworkCore;

namespace Passwordless.Common.Tests.Backup.DataFactory;

public class DbBackupContext : DbContext
{
    public DbBackupContext(DbContextOptions<DbBackupContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Person>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name);
            b.HasMany(x => x.Cars)
                .WithOne(x => x.Owner)
                .HasForeignKey(x => x.OwnerId);
        });

        modelBuilder.Entity<Car>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Owner)
                .WithMany(x => x.Cars)
                .HasForeignKey(x => x.OwnerId);
            b.Property(x => x.Make);
            b.Property(x => x.OwnerId);
        });
    }

    public DbSet<Person> People { get; set; }

    public DbSet<Car> Cars { get; set; }
}