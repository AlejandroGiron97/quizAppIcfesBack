namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Respuesta de /api/auth/login (y /api/auth/google) cuando el usuario tiene 2FA activo.
/// El cliente debe completar el login llamando a /api/auth/2fa/verify con el token y el código TOTP.
/// </summary>
public class TwoFactorRequiredResponse
{
    public bool RequiresTwoFactor { get; init; } = true;
    public required string TwoFactorToken { get; init; }
}