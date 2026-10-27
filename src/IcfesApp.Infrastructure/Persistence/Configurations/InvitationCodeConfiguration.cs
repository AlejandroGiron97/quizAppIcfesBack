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

        // Índice único: no puede haber dos códigos iguales
        builder.HasIndex(ic => ic.Code).IsUnique();

        builder.Property(ic => ic.MaxUses).HasDefaultValue(-1);
        builder.Property(ic => ic.Uses).HasDefaultValue(0);
        builder.Property(ic => ic.IsActive).HasDefaultValue(true);

        // Ignorar la propiedad calculada IsUsable (no se persiste)
        builder.Ignore(ic => ic.IsUsable);

        // Relación con Institution (School). Al borrar la institución, se borran sus códigos.
        builder.HasOne(ic => ic.Institution)
            .WithMany()
            .HasForeignKey(ic => ic.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para búsquedas rápidas por institución
        builder.HasIndex(ic => ic.InstitutionId);
    }
}