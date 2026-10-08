namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Respuesta de los endpoints que emiten sesión completa (login, 2fa/verify,
/// register-staff, google). Todos los campos son requeridos: el cliente puede
/// confiar en que están presentes.
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