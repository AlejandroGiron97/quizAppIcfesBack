using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class DisableTwoFactorRequest
{
    [Required]
    public required string Code { get; set; }
}
