using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class RegisterStaffRequest : RegisterRequest
{
    /// <summary>"Teacher" o "Admin". El registro público (/register) siempre asigna "Student".</summary>
    [Required]
    public required string Role { get; set; }
}
