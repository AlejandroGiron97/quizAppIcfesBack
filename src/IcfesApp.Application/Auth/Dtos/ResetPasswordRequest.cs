using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Token { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}
