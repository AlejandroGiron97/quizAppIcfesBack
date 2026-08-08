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
    public IReadOnlyList<string> Roles { get; private init; } = [];
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static AuthResult Success(
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc,
        IEnumerable<string> roles) => new()
    {
        Succeeded = true,
        AccessToken = accessToken,
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
        RefreshToken = refreshToken,
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
        Roles = roles.ToList()
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
