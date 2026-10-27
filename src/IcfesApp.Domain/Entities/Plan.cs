using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Catálogo de planes disponibles para la venta.
/// Los planes cambian poco; las suscripciones se crean a partir de ellos.
/// </summary>
public class Plan : BaseEntity
{
    /// <summary>Código único del plan. Ej: "FREE", "STANDARD", "INSTITUTIONAL".</summary>
    public required string Code { get; set; }

    /// <summary>Nombre visible al público. Ej: "Plan Estándar".</summary>
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Segmento al que va dirigido: Particular o Institutional.</summary>
    public PlanTarget Target { get; set; }

    /// <summary>Duración del acceso en días. Ej: 7 (FREE), 30 (STANDARD), 365 (INSTITUTIONAL).</summary>
    public int DurationDays { get; set; }

    /// <summary>Precio en la moneda configurada. 0 para planes gratuitos.</summary>
    public decimal Price { get; set; }

    /// <summary>Código ISO de la moneda. Por defecto COP.</summary>
    public string Currency { get; set; } = "COP";

    /// <summary>Permite ocultar un plan sin borrarlo.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Orden para mostrar en la UI (menor = primero).</summary>
    public int DisplayOrder { get; set; }
}