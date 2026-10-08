using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Solicitud de cambio de contraseña para un usuario autenticado.
/// Requiere la contraseña actual como medida de seguridad adicional:
/// si un atacante obtiene un access token robado, no puede cambiar la
/// contraseña sin conocer la contraseña vigente.
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}