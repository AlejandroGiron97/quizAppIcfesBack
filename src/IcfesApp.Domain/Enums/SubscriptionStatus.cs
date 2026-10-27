namespace IcfesApp.Domain.Enums;

/// <summary>
/// Estados posibles de una suscripción.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Creada pero aún no activada (ej: esperando confirmación de pago).</summary>
    Pending = 0,

    /// <summary>Activa y vigente. Es la única que da acceso al producto.</summary>
    Active = 1,

    /// <summary>Vencio por tiempo. Requiere renovación.</summary>
    Expired = 2,

    /// <summary>Cancelada por el usuario o por el administrador antes de expirar.</summary>
    Cancelled = 3,

    /// <summary>Intento de cobro fallido (aplica a renovaciones automáticas).</summary>
    PastDue = 4
}