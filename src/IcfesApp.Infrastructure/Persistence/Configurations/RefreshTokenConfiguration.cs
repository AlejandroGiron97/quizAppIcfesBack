using IcfesApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcfesApp.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.Token).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.UserId);
    }
}
