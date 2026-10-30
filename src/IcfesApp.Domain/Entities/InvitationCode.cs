using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Invitation code that lets a student register as institutional
/// without going through the payment flow. An institution Admin (or the Super Admin) generates it.
/// 
/// When registering with a valid code, the new user is linked to the code's institution
/// (their SchoolId is set to the code's InstitutionId).
/// </summary>
public class InvitationCode : BaseEntity
{
    /// <summary>Visible code the user types in the sign-up form. E.g. "COLEGIO-SJ-2026".</summary>
    public required string Code { get; set; }

    /// <summary>Institution that whoever uses this code gets linked to.</summary>
    public Guid InstitutionId { get; set; }
    public School Institution { get; set; } = null!;

    /// <summary>How many times it can be used. -1 = unlimited. 0 = can no longer be used.</summary>
    public int MaxUses { get; set; } = -1;

    /// <summary>How many times it has been used so far.</summary>
    public int Uses { get; set; }

    /// <summary>Expiration date. Null = does not expire.</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>Allows revoking the code without deleting it.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Which admin generated the code (for auditing).</summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Whether the code can be used right now:
    /// active, with uses left and not expired.
    /// </summary>
    public bool IsUsable =>
        IsActive &&
        (MaxUses < 0 || Uses < MaxUses) &&
        (ExpiresAtUtc == null || ExpiresAtUtc > DateTime.UtcNow);
}