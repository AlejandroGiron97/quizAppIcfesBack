using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// One subscription of each kind: two active institutional licenses, one active particular plan (student4)
/// and one expired free trial (student5).
/// </summary>
public class SubscriptionSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    protected override Task<bool> IsAlreadySeededAsync() => DbContext.Subscriptions.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var institutionalPlanId = await GetPlanIdAsync(DemoData.InstitutionalPlanCode);

        DbContext.Subscriptions.AddRange(
            new Subscription
            {
                InstitutionId = await GetInstitutionIdAsync(DemoData.ColegioSanJose),
                PlanId = institutionalPlanId,
                Status = SubscriptionStatus.Active,
                StartedAtUtc = nowUtc.AddDays(-30),
                ExpiresAtUtc = nowUtc.AddDays(335),
                Notes = "Licencia anual - Factura #SJ-2026-001 (demo)"
            },
            new Subscription
            {
                InstitutionId = await GetInstitutionIdAsync(DemoData.LiceoModerno),
                PlanId = institutionalPlanId,
                Status = SubscriptionStatus.Active,
                StartedAtUtc = nowUtc.AddDays(-15),
                ExpiresAtUtc = nowUtc.AddDays(350),
                Notes = "Licencia anual - Factura #LM-2026-002 (demo)"
            });

        var student4Id = await FindUserIdAsync(DemoData.Student4Email);
        if (student4Id is not null)
        {
            DbContext.Subscriptions.Add(new Subscription
            {
                UserId = student4Id,
                PlanId = await GetPlanIdAsync(DemoData.StandardPlanCode),
                Status = SubscriptionStatus.Active,
                StartedAtUtc = nowUtc.AddDays(-10),
                ExpiresAtUtc = nowUtc.AddDays(20),
                AutoRenew = true,
                Notes = "Suscripción demo - Stripe"
            });
        }

        var student5Id = await FindUserIdAsync(DemoData.Student5Email);
        if (student5Id is not null)
        {
            DbContext.Subscriptions.Add(new Subscription
            {
                UserId = student5Id,
                PlanId = await GetPlanIdAsync(DemoData.FreePlanCode),
                Status = SubscriptionStatus.Expired,
                StartedAtUtc = nowUtc.AddDays(-30),
                ExpiresAtUtc = nowUtc.AddDays(-23),
                Notes = "Trial gratuito vencido"
            });
        }

        await DbContext.SaveChangesAsync();
    }
}
