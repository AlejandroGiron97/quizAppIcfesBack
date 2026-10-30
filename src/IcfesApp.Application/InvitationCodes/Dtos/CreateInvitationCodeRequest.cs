using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.InvitationCodes.Dtos;

public class CreateInvitationCodeRequest
{
    /// <summary>
    /// Visible code. If left empty, the system generates one automatically
    /// from the institution name + 6 random characters.
    /// </summary>
    [MaxLength(50)]
    public string? Code { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }

    /// <summary>
    /// Maximum number of uses. -1 = unlimited.
    /// </summary>
    public int MaxUses { get; set; } = -1;

    /// <summary>
    /// Expiration date. Null = never expires.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }
}