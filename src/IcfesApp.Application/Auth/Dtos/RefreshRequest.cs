using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class RefreshRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
