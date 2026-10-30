namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Response of /api/auth/refresh: session tokens only.
/// It intentionally does not include user data — the client already has it from login
/// and can refresh it with /api/auth/session if needed.
/// This keeps the payload small for an endpoint that is called silently every ~15 min.
/// </summary>
public class AuthSessionResponse
{
    public string TokenType { get; init; } = "Bearer";
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiresAtUtc { get; init; }
}