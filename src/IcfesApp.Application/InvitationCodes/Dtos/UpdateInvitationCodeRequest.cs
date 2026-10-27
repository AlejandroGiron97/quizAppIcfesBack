namespace IcfesApp.Application.InvitationCodes.Dtos;

public class UpdateInvitationCodeRequest
{
    /// <summary>
    /// Número máximo de usos. -1 = ilimitado. No puede ser menor a los usos ya realizados.
    /// </summary>
    public int MaxUses { get; set; }

    /// <summary>
    /// Fecha de expiración. Null = nunca expira.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Permite reactivar un código revocado (true) o desactivarlo (false).
    /// </summary>
    public bool IsActive { get; set; }
}