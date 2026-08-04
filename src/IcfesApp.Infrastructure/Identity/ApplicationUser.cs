using IcfesApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
}
