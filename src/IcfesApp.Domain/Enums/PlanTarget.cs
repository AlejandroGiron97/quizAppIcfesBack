namespace IcfesApp.Domain.Enums;

/// <summary>
/// A qué segmento va dirigido un plan.
/// </summary>
public enum PlanTarget
{
    /// <summary>Plan para usuarios que acceden individualmente (pagan autoservicio).</summary>
    Particular = 0,

    /// <summary>Plan para instituciones educativas (licencia anual, pago manual).</summary>
    Institutional = 1
}