using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passwordless.Fido2.MetadataService.Models;

namespace Passwordless.Fido2.MetadataService.Db.Configurations;

public class AuthenticatorEntityTypeConfiguration : IEntityTypeConfiguration<Authenticator>
{
    public void Configure(EntityTypeBuilder<Authenticator> builder)
    {
        builder.HasKey(b => b.AaGuid);
        
        builder.Property(b => b.AaGuid)
            .IsRequired();
        
        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(50);
    }
}