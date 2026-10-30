using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class School : BaseEntity
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