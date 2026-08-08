using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class ResendConfirmationRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }
}
