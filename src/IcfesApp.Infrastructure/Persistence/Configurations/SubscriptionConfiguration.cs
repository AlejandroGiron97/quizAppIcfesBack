using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcfesApp.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions", t =>
        {
            // EXACTAMENTE una de las dos FKs debe estar poblada.
            // (UserId NOT NULL AND InstitutionId NULL) OR (UserId NULL AND InstitutionId NOT NULL)
            t.HasCheckConstraint(
                "CK_Subscription_Owner",
                "(\"UserId\" IS NOT NULL AND \"InstitutionId\" IS NULL) OR (\"UserId\" IS NULL AND \"InstitutionId\" IS NOT NULL)");
        });

        builder.Ignore(s => s.IsCurrentlyActive);

        builder.HasOne(s => s.Institution)
            .WithMany()
            .HasForeignKey(s => s.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.InstitutionId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ExpiresAtUtc);
    }
}