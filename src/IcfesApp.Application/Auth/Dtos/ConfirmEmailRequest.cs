using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class ConfirmEmailRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Token { get; set; }
}
