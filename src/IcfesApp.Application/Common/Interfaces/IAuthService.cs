using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

public interface IAuthService
{
    // Registro
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Un Admin crea un Teacher/Admin. Devuelve el usuario creado (no emite tokens para él).</summary>
    Task<Result<UserInfoDto>> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);

    // Sesión
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Datos actualizados del usuario autenticado, leídos de la base de datos.</summary>
    Task<UserInfoDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    // Contraseña
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    // Confirmación de correo
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);

    // Doble factor (2FA)
    Task<TwoFactorSetupDto?> GetTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<Result> DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<AuthResult> VerifyTwoFactorAsync(string twoFactorToken, string code, CancellationToken cancellationToken = default);
}
