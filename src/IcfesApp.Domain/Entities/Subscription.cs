using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Suscripción activa de un usuario particular o de una institución.
/// 
/// REGLA: exactamente una de las dos FKs (UserId o InstitutionId) está poblada.
/// Nunca las dos, nunca ninguna. Se valida con check constraint en la BD.
/// </summary>
public class Subscription : BaseEntity
{
    /// <summary>Usuario particular dueño. Null si es institucional.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Institución dueña. Null si es particular.</summary>
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