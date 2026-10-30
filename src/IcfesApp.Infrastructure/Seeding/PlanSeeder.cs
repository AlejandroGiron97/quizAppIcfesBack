using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>The plans catalog shown on the pricing page (prices in COP).</summary>
public class PlanSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    private const string Currency = "COP";

    protected override Task<bool> IsAlreadySeededAsync() => DbContext.Plans.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        DbContext.Plans.AddRange(
            new Plan { Code = DemoData.FreePlanCode, Name = "Prueba gratuita", Description = "7 días de acceso completo. Se activa una vez por usuario.", Target = PlanTarget.Particular, DurationDays = 7, Price = 0m, Currency = Currency, DisplayOrder = 1 },
            new Plan { Code = DemoData.StandardPlanCode, Name = "Plan Estándar", Description = "Acceso completo por 30 días.", Target = PlanTarget.Particular, DurationDays = 30, Price = 29900m, Currency = Currency, DisplayOrder = 2 },
            new Plan { Code = DemoData.SemestralPlanCode, Name = "Plan Semestral", Description = "Acceso completo por 180 días. Ahorra 17%.", Target = PlanTarget.Particular, DurationDays = 180, Price = 149900m, Currency = Currency, DisplayOrder = 3 },
            new Plan { Code = DemoData.InstitutionalPlanCode, Name = "Licencia Institucional", Description = "Acceso completo por 365 días. Hasta 200 estudiantes.", Target = PlanTarget.Institutional, DurationDays = 365, Price = 1800000m, Currency = Currency, DisplayOrder = 1 });

        await DbContext.SaveChangesAsync();
    }
}
