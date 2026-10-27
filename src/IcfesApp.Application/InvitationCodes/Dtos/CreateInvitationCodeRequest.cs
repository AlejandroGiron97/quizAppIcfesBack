using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.InvitationCodes.Dtos;

public class CreateInvitationCodeRequest
{
    /// <summary>
    /// Código visible. Si se deja vacío, el sistema genera uno automáticamente
    /// con el nombre de la institución + 6 caracteres aleatorios.
    /// </summary>
    [MaxLength(50)]
    public string? Code { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }

    /// <summary>
    /// Número máximo de usos. -1 = ilimitado.
    /// </summary>
    public int MaxUses { get; set; } = -1;

    /// <summary>
    /// Fecha de expiración. Null = nunca expira.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }
}