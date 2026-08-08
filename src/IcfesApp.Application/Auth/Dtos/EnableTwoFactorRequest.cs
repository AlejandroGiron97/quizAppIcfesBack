using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class EnableTwoFactorRequest
{
    [Required]
    public required string Code { get; set; }
}
