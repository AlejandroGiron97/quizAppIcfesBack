using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    /// <summary>
    /// Código de invitación opcional. Si viene y es válido, el usuario queda vinculado
    /// a la institución del código (Type = Institutional). Si no viene, es Particular.
    /// </summary>
    [MaxLength(50)]
    public string? InvitationCode { get; set; }
}