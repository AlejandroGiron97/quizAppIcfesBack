using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcfesApp.Infrastructure.Persistence.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Domain).HasMaxLength(150);
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
    }
}