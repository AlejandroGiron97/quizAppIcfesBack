using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Active subscription of a particular user or of an institution.
/// 
/// RULE: exactly one of the two FKs (UserId or InstitutionId) is set.
/// Never both, never neither. Enforced with a check constraint in the database.
/// </summary>
public class Subscription : BaseEntity
{
    /// <summary>Owning particular user. Null if institutional.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Owning institution. Null if particular.</summary>
    public Guid? InstitutionId { get; set; }
    public School? Institution { get; set; }

    /// <summary>Plan contratado.</summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

    public DateTime StartedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public bool AutoRenew { get; set; } = false;
    public string? Notes { get; set; }

    public bool IsCurrentlyActive =>
        Status == SubscriptionStatus.Active && ExpiresAtUtc > DateTime.UtcNow;
}