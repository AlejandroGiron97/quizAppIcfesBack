namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Response of the endpoints that issue a full session (login, 2fa/verify,
/// google). Every field is required: the client can
/// rely on them being present.
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