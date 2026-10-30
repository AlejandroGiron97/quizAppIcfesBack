using IcfesApp.Domain.Enums;

namespace IcfesApp.Infrastructure.Identity;

/// <summary>
/// Creates Identity users (Factory pattern). It is the ONLY place that does "new ApplicationUser",
/// so the creation rules live in a single spot:
///   - UserName is always the email.
///   - The type (Particular / Institutional) depends on whether the user belongs to an institution.
///   - Who needs to confirm their email and who does not.
///
/// The role is NOT assigned here: it requires the user to already exist in the database (userManager.AddToRoleAsync).
/// </summary>
public static class ApplicationUserFactory
{
    /// <summary>Student who signs up by themselves. Must confirm their email before signing in.</summary>
    public static ApplicationUser CreateSelfRegisteredStudent(
        string email, string firstName, string lastName, Guid? institutionId) =>
        Create(email, firstName, lastName, institutionId, emailConfirmed: false);

    /// <summary>Teacher or Admin created by an Admin. The email is considered confirmed.</summary>
    public static ApplicationUser CreateStaff(string email, string firstName, string lastName) =>
        Create(email, firstName, lastName, institutionId: null, emailConfirmed: true);

    /// <summary>
    /// User signing in with Google for the first time. Google already verified the email.
    /// If Google does not send the name, the email is used so it is not left empty.
    /// </summary>
    public static ApplicationUser CreateFromGoogle(string email, string? givenName, string? familyName) =>
        Create(
            email,
            firstName: string.IsNullOrWhiteSpace(givenName) ? email : givenName,
            lastName: familyName ?? string.Empty,
            institutionId: null,
            emailConfirmed: true);

    /// <summary>User created by the seeders (default admin and demo data). Email already confirmed.</summary>
    public static ApplicationUser CreatePreconfirmed(
        string email, string firstName, string lastName, Guid? institutionId = null) =>
        Create(email, firstName, lastName, institutionId, emailConfirmed: true);

    private static ApplicationUser Create(
        string email, string firstName, string lastName, Guid? institutionId, bool emailConfirmed) => new()
    {
        UserName = email,
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        EmailConfirmed = emailConfirmed,
        SchoolId = institutionId,
        Type = institutionId.HasValue ? UserType.Institutional : UserType.Particular
    };
}
