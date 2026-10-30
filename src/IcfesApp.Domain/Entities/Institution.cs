using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// A school ("colegio") that buys a yearly license. Its students and teachers are linked to it
/// (ApplicationUser.InstitutionId) and it can generate invitation codes.
/// Named "School" (table "Schools") before refactor phase 6.
/// </summary>
public class Institution : BaseEntity
{
    public required string Name { get; set; }

    /// <summary>Institutional domain (e.g. "colegiosanjose.edu.co").</summary>
    public string? Domain { get; set; }

    public string? City { get; set; }

    /// <summary>Allows deactivating without deleting.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Which SuperAdmin created it. Auditing.</summary>
    public Guid? CreatedByUserId { get; set; }
}