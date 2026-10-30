using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class RegisterStaffRequest : RegisterRequest
{
    /// <summary>"Teacher" or "Admin". Public sign-up (/register) always assigns "Student".</summary>
    [Required]
    public required string Role { get; set; }
}
