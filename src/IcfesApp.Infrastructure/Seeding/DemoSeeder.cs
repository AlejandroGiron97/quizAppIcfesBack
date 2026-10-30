using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Base class of every demo data seeder (Template Method pattern).
///
/// <see cref="SeedAsync"/> fixes the steps: skip if the data is already there, otherwise insert it.
/// Each subclass only says how to detect its data (<see cref="IsAlreadySeededAsync"/>) and how to create it
/// (<see cref="InsertDataAsync"/>). Running the seeders again on an existing database never duplicates data.
///
/// To add demo data for a new entity: create a subclass and add it to the list in <see cref="DataSeeder"/>.
/// </summary>
public abstract class DemoSeeder(ApplicationDbContext dbContext)
{
    protected ApplicationDbContext DbContext { get; } = dbContext;

    /// <summary>The template method: the order of the steps is the same for every seeder.</summary>
    public async Task SeedAsync()
    {
        if (await IsAlreadySeededAsync())
        {
            return;
        }

        await InsertDataAsync();
    }

    /// <summary>True when the data this seeder creates already exists.</summary>
    protected abstract Task<bool> IsAlreadySeededAsync();

    protected abstract Task InsertDataAsync();

    // ─────────────────────────────────────────────────────────────
    // Lookups shared by the seeders (data created by an earlier seeder)
    // ─────────────────────────────────────────────────────────────

    protected Task<Guid> GetInstitutionIdAsync(string institutionName) =>
        DbContext.Institutions
            .Where(institution => institution.Name == institutionName)
            .Select(institution => institution.Id)
            .SingleAsync();

    protected Task<Guid> GetSubjectIdAsync(string subjectName) =>
        DbContext.Subjects
            .Where(subject => subject.Name == subjectName)
            .Select(subject => subject.Id)
            .SingleAsync();

    protected Task<Guid> GetPlanIdAsync(string planCode) =>
        DbContext.Plans
            .Where(plan => plan.Code == planCode)
            .Select(plan => plan.Id)
            .SingleAsync();

    /// <summary>Id of a user by email, or null if that demo user does not exist.</summary>
    protected async Task<Guid?> FindUserIdAsync(string email)
    {
        var userId = await DbContext.Users
            .Where(user => user.Email == email)
            .Select(user => user.Id)
            .FirstOrDefaultAsync();

        return userId == Guid.Empty ? null : userId;
    }
}
