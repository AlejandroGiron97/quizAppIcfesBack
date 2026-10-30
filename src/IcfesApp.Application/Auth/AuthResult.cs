using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Auth;

/// <summary>
/// Result of an authentication flow. Inherits the success state, the errors and their type
/// from <see cref="Result"/>, and adds the data of the issued session.
/// </summary>
public class AuthResult : Result
{
    public bool RequiresEmailConfirmation { get; private init; }
    public bool RequiresTwoFactor { get; private init; }
    public string? TwoFactorToken { get; private init; }
    public string? AccessToken { get; private init; }
    public DateTime? AccessTokenExpiresAtUtc { get; private init; }
    public string? RefreshToken { get; private init; }
    public DateTime? RefreshTokenExpiresAtUtc { get; private init; }

    /// <summary>
    /// User data. Set whenever tokens are issued; null for "2FA required" and "registration pending".
    /// </summary>
    public UserInfoDto? User { get; private init; }

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

    public new static AuthResult Failed(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation) => new()
    {
        ErrorType = errorType,
        Errors = errors.ToList()
    };
}
