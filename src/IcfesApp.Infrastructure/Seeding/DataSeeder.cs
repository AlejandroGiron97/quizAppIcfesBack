using Microsoft.Extensions.DependencyInjection;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Entry point of the demo data. Runs every <see cref="DemoSeeder"/> in order.
/// Only runs in Development (see Program.cs): it creates users with a known password.
///
/// The order matters because later seeders use data created by earlier ones
/// (e.g. users need institutions, sessions need users and questions).
/// </summary>
public static class DataSeeder
{
    private static readonly Type[] SeedersInOrder =
    [
        typeof(InstitutionSeeder),
        typeof(SubjectSeeder),
        typeof(DemoUserSeeder),
        typeof(InvitationCodeSeeder),
        typeof(IcfesQuestionSeeder),
        typeof(PracticeSessionSeeder),
        typeof(PlanSeeder),
        typeof(SubscriptionSeeder)
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        foreach (var seederType in SeedersInOrder)
        {
            // The seeders are not registered in DI: they are created here with their dependencies
            // (DbContext, UserManager, logger...) resolved from the current scope.
            var seeder = (DemoSeeder)ActivatorUtilities.CreateInstance(serviceProvider, seederType);
            await seeder.SeedAsync();
        }
    }
}
