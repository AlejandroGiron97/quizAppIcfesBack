using IcfesApp.Domain.Entities;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

public class InstitutionSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    protected override Task<bool> IsAlreadySeededAsync() => DbContext.Institutions.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        DbContext.Institutions.AddRange(
            new Institution { Name = DemoData.ColegioSanJose, City = "Bogotá" },
            new Institution { Name = DemoData.LiceoModerno, City = "Medellín" },
            new Institution { Name = DemoData.LaEsperanza, City = "Cali" });

        await DbContext.SaveChangesAsync();
    }
}
