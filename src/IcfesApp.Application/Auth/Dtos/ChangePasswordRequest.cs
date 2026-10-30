using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Password change request for an authenticated user.
/// Requires the current password as an extra security measure:
/// if an attacker gets hold of a stolen access token, they cannot change the
/// password without knowing the current one.
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}