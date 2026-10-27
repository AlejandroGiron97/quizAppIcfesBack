namespace IcfesApp.Application.Subscriptions.Dtos;

public class SubscriptionDto
{
    public Guid Id { get; set; }

    /// <summary>"Particular" o "Institutional"</summary>
    public required string OwnerType { get; set; }

    public Guid? UserId { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }

    // Datos del plan (aplanados para evitar consultas extra en el front)
    public Guid PlanId { get; set; }
    public required string PlanCode { get; set; }
    public required string PlanName { get; set; }

    /// <summary>"Pending", "Active", "Expired", "Cancelled", "PastDue"</summary>
    public required string Status { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public bool AutoRenew { get; set; }

    /// <summary>Da acceso activo ahora mismo (Active + no vencida).</summary>
    public bool IsCurrentlyActive { get; set; }

    /// <summary>Días restantes. Negativo si ya expiró.</summary>
    public int DaysRemaining { get; set; }
}