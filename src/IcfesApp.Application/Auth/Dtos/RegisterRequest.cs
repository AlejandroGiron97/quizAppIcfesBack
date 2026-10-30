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
    /// Optional invitation code. When present and valid, the user is linked
    /// to the code's institution (Type = Institutional). When absent, the user is Particular.
    /// </summary>
    [MaxLength(50)]
    public string? InvitationCode { get; set; }
}