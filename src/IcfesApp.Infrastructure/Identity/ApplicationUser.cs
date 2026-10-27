using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Modelo de acceso del usuario. Por defecto Particular; se cambia a Institutional
    /// cuando el usuario se registra o aplica un código de invitación válido.
    /// </summary>
    public UserType Type { get; set; } = UserType.Particular;

    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
}