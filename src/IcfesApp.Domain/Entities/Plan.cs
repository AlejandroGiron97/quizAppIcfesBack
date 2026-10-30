using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Catalog of plans available for sale.
/// Plans rarely change; subscriptions are created from them.
/// </summary>
public class Plan : BaseEntity
{
    /// <summary>Unique plan code. E.g. "FREE", "STANDARD", "INSTITUTIONAL".</summary>
    public required string Code { get; set; }

    /// <summary>Public display name. E.g. "Plan Estándar".</summary>
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Target segment: Particular or Institutional.</summary>
    public PlanTarget Target { get; set; }

    /// <summary>Access duration in days. E.g. 7 (FREE), 30 (STANDARD), 365 (INSTITUTIONAL).</summary>
    public int DurationDays { get; set; }

    /// <summary>Price in the configured currency. 0 for free plans.</summary>
    public decimal Price { get; set; }

    /// <summary>ISO currency code. COP by default.</summary>
    public string Currency { get; set; } = "COP";

    /// <summary>Allows hiding a plan without deleting it.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Display order in the UI (lower = first).</summary>
    public int DisplayOrder { get; set; }
}