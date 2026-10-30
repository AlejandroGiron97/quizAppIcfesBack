using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcfesApp.Infrastructure.Persistence.Configurations;

public class InvitationCodeConfiguration : IEntityTypeConfiguration<InvitationCode>
{
    public void Configure(EntityTypeBuilder<InvitationCode> builder)
    {
        builder.ToTable("InvitationCodes");

        builder.Property(ic => ic.Code)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index: there cannot be two equal codes
        builder.HasIndex(ic => ic.Code).IsUnique();

        builder.Property(ic => ic.MaxUses).HasDefaultValue(-1);
        builder.Property(ic => ic.Uses).HasDefaultValue(0);
        builder.Property(ic => ic.IsActive).HasDefaultValue(true);

        // Ignore the computed IsUsable property (not persisted)
        builder.Ignore(ic => ic.IsUsable);

        // Relationship with Institution (School). Deleting the institution deletes its codes.
        builder.HasOne(ic => ic.Institution)
            .WithMany()
            .HasForeignKey(ic => ic.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for fast lookups by institution
        builder.HasIndex(ic => ic.InstitutionId);
    }
}