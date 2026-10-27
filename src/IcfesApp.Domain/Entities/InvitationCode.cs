using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Código de invitación que permite a un estudiante registrarse como institucional
/// sin pasar por el flujo de pago. Un Admin de institución (o el Super Admin) lo genera.
/// 
/// Al registrarse con un código válido, el nuevo usuario queda vinculado a la institución
/// del código (su SchoolId se establece al InstitutionId del código).
/// </summary>
public class InvitationCode : BaseEntity
{
    /// <summary>Código visible que el usuario introduce en el formulario de registro. Ej: "COLEGIO-SJ-2026".</summary>
    public required string Code { get; set; }

    /// <summary>Institución a la que se vincula quien use este código.</summary>
    public Guid InstitutionId { get; set; }
    public School Institution { get; set; } = null!;

    /// <summary>Cuántas veces puede usarse. -1 = ilimitado. 0 = ya no se puede usar.</summary>
    public int MaxUses { get; set; } = -1;

    /// <summary>Cuántas veces se ha usado ya.</summary>
    public int Uses { get; set; }

    /// <summary>Fecha de expiración. Null = no expira.</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>Permite revocar el código sin borrarlo.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Qué admin generó el código (para auditoría).</summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Indica si el código puede usarse ahora mismo:
    /// activo, con usos disponibles y sin haber expirado.
    /// </summary>
    public bool IsUsable =>
        IsActive &&
        (MaxUses < 0 || Uses < MaxUses) &&
        (ExpiresAtUtc == null || ExpiresAtUtc > DateTime.UtcNow);
}