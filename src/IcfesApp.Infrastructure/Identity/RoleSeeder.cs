using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IcfesApp.Infrastructure.Identity;

public static class RoleSeeder
{
    // Solo para bootstrap en desarrollo: cambiar esta contraseña (o borrar el usuario) antes de ir a producción.
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

        var admin = new ApplicationUser
        {
            UserName = DefaultAdminEmail,
            Email = DefaultAdminEmail,
            FirstName = "Admin",
            LastName = "IcfesApp",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, DefaultAdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
