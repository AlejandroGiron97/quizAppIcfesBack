using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class TwoFactorLoginRequest
{
    [Required]
    public required string TwoFactorToken { get; set; }

    [Required]
    public required string Code { get; set; }
}
