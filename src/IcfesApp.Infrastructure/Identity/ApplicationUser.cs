using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User access model. Particular by default; it changes to Institutional
    /// when the user registers with or applies a valid invitation code.
    /// </summary>
    public UserType Type { get; set; } = UserType.Particular;

    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
}