namespace IcfesApp.Application.Subscriptions.Dtos;

public class SubscriptionDto
{
    public Guid Id { get; set; }

    /// <summary>"Particular" or "Institutional"</summary>
    public required string OwnerType { get; set; }

    public Guid? UserId { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }

    // Plan data (flattened to avoid extra requests from the frontend)
    public Guid PlanId { get; set; }
    public required string PlanCode { get; set; }
    public required string PlanName { get; set; }

    /// <summary>"Pending", "Active", "Expired", "Cancelled", "PastDue"</summary>
    public required string Status { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public bool AutoRenew { get; set; }

    /// <summary>Grants active access right now (Active + not expired).</summary>
    public bool IsCurrentlyActive { get; set; }

    /// <summary>Days remaining. Negative if it already expired.</summary>
    public int DaysRemaining { get; set; }
}