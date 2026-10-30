using IcfesApp.Domain.Entities;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

public class InvitationCodeSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    private const int UnlimitedUses = -1;

    protected override Task<bool> IsAlreadySeededAsync() => DbContext.InvitationCodes.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        var colegioSanJoseId = await GetInstitutionIdAsync(DemoData.ColegioSanJose);
        var liceoModernoId = await GetInstitutionIdAsync(DemoData.LiceoModerno);
        var sixMonthsFromNow = DateTime.UtcNow.AddMonths(6);

        DbContext.InvitationCodes.AddRange(
            new InvitationCode { Code = "COLEGIO-SJ-2026", InstitutionId = colegioSanJoseId, MaxUses = 100, ExpiresAtUtc = sixMonthsFromNow },
            new InvitationCode { Code = "LICEOMODERNO-2026", InstitutionId = liceoModernoId, MaxUses = 50, ExpiresAtUtc = sixMonthsFromNow },
            new InvitationCode { Code = "PRUEBA-ILIMITADO", InstitutionId = colegioSanJoseId, MaxUses = UnlimitedUses, ExpiresAtUtc = null });

        await DbContext.SaveChangesAsync();
    }
}
