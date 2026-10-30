using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Demo teachers and students, all with the password <see cref="DemoData.DefaultPassword"/>:
///   - teacher1 / student1 / student2 → Colegio San José (institutional)
///   - teacher2 / student3            → Liceo Moderno (institutional)
///   - student4                       → particular with an active subscription
///   - student5                       → particular with an expired free trial
/// </summary>
public class DemoUserSeeder(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    : DemoSeeder(dbContext)
{
    /// <summary>Each user is checked one by one, so a missing user is created even if the others exist.</summary>
    protected override Task<bool> IsAlreadySeededAsync() => Task.FromResult(false);

    protected override async Task InsertDataAsync()
    {
        var colegioSanJoseId = await GetInstitutionIdAsync(DemoData.ColegioSanJose);
        var liceoModernoId = await GetInstitutionIdAsync(DemoData.LiceoModerno);

        await EnsureUserAsync(DemoData.Teacher1Email, "Carlos", "Ramírez", Roles.Teacher, colegioSanJoseId);
        await EnsureUserAsync(DemoData.Teacher2Email, "María", "González", Roles.Teacher, liceoModernoId);

        await EnsureUserAsync(DemoData.Student1Email, "Juan", "Pérez", Roles.Student, colegioSanJoseId);
        await EnsureUserAsync(DemoData.Student2Email, "Pepito", "Rodríguez", Roles.Student, colegioSanJoseId);
        await EnsureUserAsync(DemoData.Student3Email, "Ana", "Martínez", Roles.Student, liceoModernoId);
        await EnsureUserAsync(DemoData.Student4Email, "Diego", "López", Roles.Student, institutionId: null);
        await EnsureUserAsync(DemoData.Student5Email, "Sofía", "Hernández", Roles.Student, institutionId: null);
    }

    private async Task EnsureUserAsync(string email, string firstName, string lastName, string role, Guid? institutionId)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var demoUser = ApplicationUserFactory.CreatePreconfirmed(email, firstName, lastName, institutionId);

        var createUserResult = await userManager.CreateAsync(demoUser, DemoData.DefaultPassword);
        if (createUserResult.Succeeded)
        {
            await userManager.AddToRoleAsync(demoUser, role);
        }
    }
}
