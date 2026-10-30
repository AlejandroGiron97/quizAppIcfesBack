namespace IcfesApp.Application.InvitationCodes.Dtos;

public class UpdateInvitationCodeRequest
{
    /// <summary>
    /// Maximum number of uses. -1 = unlimited. Cannot be lower than the uses already made.
    /// </summary>
    public int MaxUses { get; set; }

    /// <summary>
    /// Expiration date. Null = never expires.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Allows reactivating a revoked code (true) or deactivating it (false).
    /// </summary>
    public bool IsActive { get; set; }
}