using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
