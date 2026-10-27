using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcfesApp.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("COP");
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
    }
}