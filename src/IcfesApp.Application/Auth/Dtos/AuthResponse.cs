namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Response of every endpoint that issues a session: login, google-login, 2fa/verify-login and refresh-token.
/// Every field is required: the client can rely on them being present.
/// </summary>
public class AuthResponse
{
    public string TokenType { get; init; } = "Bearer";
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiresAtUtc { get; init; }
    public required UserInfoDto User { get; init; }
}