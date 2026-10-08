using IcfesApp.Application.Auth.Dtos;

namespace IcfesApp.Application.Auth;

public class AuthResult
{
    public bool Succeeded { get; private init; }
    public bool RequiresEmailConfirmation { get; private init; }
    public bool RequiresTwoFactor { get; private init; }
    public string? TwoFactorToken { get; private init; }
    public string? AccessToken { get; private init; }
    public DateTime? AccessTokenExpiresAtUtc { get; private init; }
    public string? RefreshToken { get; private init; }
    public DateTime? RefreshTokenExpiresAtUtc { get; private init; }

    /// <summary>
    /// Datos del usuario. Null cuando la emisión de tokens no los incluye (ej: /refresh).
    /// </summary>
    public UserInfoDto? User { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static AuthResult Success(
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc,
        UserInfoDto? user = null) => new()
        {
            Succeeded = true,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
            User = user
        };

    public static AuthResult RegistrationPending() => new()
    {
        Succeeded = true,
        RequiresEmailConfirmation = true
    };

    public static AuthResult TwoFactorRequired(string twoFactorToken) => new()
    {
        Succeeded = true,
        RequiresTwoFactor = true,
        TwoFactorToken = twoFactorToken
    };

    public static AuthResult Failed(IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };
}