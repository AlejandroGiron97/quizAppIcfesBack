using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IcfesApp.Infrastructure.Identity;

public static class RoleSeeder
{
    // Development bootstrap only: change this password (or delete the user) before going to production.
    public const string DefaultAdminEmail = "admin@icfesapp.com";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { Roles.Admin, Roles.Teacher, Roles.Student })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (existingAdmins.Count > 0)
        {
            return;
        }

        var defaultAdmin = ApplicationUserFactory.CreatePreconfirmed(DefaultAdminEmail, "Admin", "IcfesApp");

        var createAdminResult = await userManager.CreateAsync(defaultAdmin, DefaultAdminPassword);
        if (createAdminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(defaultAdmin, Roles.Admin);
        }
    }
}
