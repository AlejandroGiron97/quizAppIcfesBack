namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Public data of the authenticated user. Included in every login/refresh response
/// so the client does not need to call /session right afterwards.
/// </summary>
public class UserInfoDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>
    /// "Particular" or "Institutional". Determines whether the user pays a subscription
    /// or belongs to an institution (does not pay).
    /// </summary>
    public required string Type { get; init; }

    public Guid? InstitutionId { get; init; }
}