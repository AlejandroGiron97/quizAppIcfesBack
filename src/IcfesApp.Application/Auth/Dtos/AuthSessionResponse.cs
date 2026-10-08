namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Respuesta de /api/auth/refresh: solo tokens de sesión.
/// No incluye datos del usuario a propósito — el cliente ya los tiene del login
/// y puede refrescarlos con /api/auth/session si los necesita.
/// Esto reduce el payload de un endpoint que se llama silenciosamente cada ~15 min.
/// </summary>
public class AuthSessionResponse
{
    public string TokenType { get; init; } = "Bearer";
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiresAtUtc { get; init; }
}