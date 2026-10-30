namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Response of /api/auth/login (and /api/auth/google) when the user has 2FA enabled.
/// The client must finish signing in by calling /api/auth/2fa/verify with the token and the TOTP code.
/// </summary>
public class TwoFactorRequiredResponse
{
    public bool RequiresTwoFactor { get; init; } = true;
    public required string TwoFactorToken { get; init; }
}