using IcfesApp.Domain.Entities;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>The five subjects of the ICFES exam.</summary>
public class SubjectSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    protected override Task<bool> IsAlreadySeededAsync() => DbContext.Subjects.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        DbContext.Subjects.AddRange(
            new Subject { Name = DemoData.Mathematics, Description = "Razonamiento cuantitativo y resolución de problemas" },
            new Subject { Name = DemoData.CriticalReading, Description = "Comprensión lectora y análisis de textos" },
            new Subject { Name = DemoData.NaturalSciences, Description = "Biología, Química y Física" },
            new Subject { Name = DemoData.SocialSciences, Description = "Historia, Geografía y Constitución" },
            new Subject { Name = DemoData.English, Description = "Comprensión y uso del idioma inglés" });

        await DbContext.SaveChangesAsync();
    }
}
