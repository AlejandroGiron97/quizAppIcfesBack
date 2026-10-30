using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    private const string BearerTokenType = "Bearer";

    // ─────────────────────────────────────────────────────────────
    // Registro
    // ─────────────────────────────────────────────────────────────

    /// <summary>Registro público. Siempre crea un usuario con rol Student.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegistrationPendingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var registrationResult = await authService.RegisterAsync(request, cancellationToken);
        if (!registrationResult.Succeeded)
        {
            return ToErrorResponse(registrationResult);
        }

        return Ok(new RegistrationPendingResponse
        {
            Message = "Revisa tu correo para confirmar tu cuenta antes de iniciar sesión."
        });
    }

    /// <summary>
    /// Registro de staff (Teacher/Admin). Solo accesible por un Admin.
    /// Devuelve el usuario creado; no emite tokens para él (el admin sigue con su propia sesión).
    /// </summary>
    [HttpPost("register-staff")]
    [Authorize(Policy = Policies.RequireAdmin)]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request, CancellationToken cancellationToken)
        => ToActionResult(await authService.RegisterStaffAsync(request, cancellationToken));

    // ─────────────────────────────────────────────────────────────
    // Sesión
    // ─────────────────────────────────────────────────────────────

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await authService.LoginAsync(request, cancellationToken));

    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await authService.GoogleLoginAsync(request.IdToken, cancellationToken));

    /// <summary>
    /// Devuelve los datos actualizados del usuario autenticado (leídos de la base de datos).
    /// Se usa al recargar la app para obtener estado fresco sin rotar tokens.
    /// </summary>
    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        var currentUser = await authService.GetCurrentUserAsync(CurrentUserId, cancellationToken);
        return currentUser is null ? Unauthorized() : Ok(currentUser);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var refreshResult = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (!refreshResult.Succeeded)
        {
            return ToErrorResponse(refreshResult);
        }

        return Ok(new AuthSessionResponse
        {
            TokenType = BearerTokenType,
            AccessToken = refreshResult.AccessToken!,
            ExpiresAtUtc = refreshResult.AccessTokenExpiresAtUtc!.Value,
            RefreshToken = refreshResult.RefreshToken!,
            RefreshTokenExpiresAtUtc = refreshResult.RefreshTokenExpiresAtUtc!.Value
        });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(RefreshRequest request, CancellationToken cancellationToken)
    {
        await authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────
    // Contraseña
    // ─────────────────────────────────────────────────────────────

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Si el correo existe, se enviarán instrucciones para restablecer la contraseña." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var resetPasswordResult = await authService.ResetPasswordAsync(request, cancellationToken);
        return resetPasswordResult.Succeeded
            ? Ok(new { message = "Contraseña restablecida correctamente. Ya puedes iniciar sesión." })
            : ToErrorResponse(resetPasswordResult);
    }

    /// <summary>
    /// Cambio de contraseña para un usuario autenticado.
    /// Requiere la contraseña actual como medida de seguridad.
    /// Al completarse, revoca todos los refresh tokens activos del usuario.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var changePasswordResult = await authService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return changePasswordResult.Succeeded
            ? Ok(new { message = "Contraseña actualizada correctamente. Por seguridad, deberás iniciar sesión de nuevo en otros dispositivos." })
            : ToErrorResponse(changePasswordResult);
    }

    // ─────────────────────────────────────────────────────────────
    // Confirmación de correo
    // ─────────────────────────────────────────────────────────────

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var confirmEmailResult = await authService.ConfirmEmailAsync(request, cancellationToken);
        return confirmEmailResult.Succeeded
            ? Ok(new { message = "Correo confirmado correctamente. Ya puedes iniciar sesión." })
            : ToErrorResponse(confirmEmailResult);
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await authService.ResendConfirmationEmailAsync(request, cancellationToken);
        return Ok(new { message = "Si el correo existe y no ha sido confirmado, se enviará un nuevo enlace." });
    }

    // ─────────────────────────────────────────────────────────────
    // Doble factor (2FA)
    // ─────────────────────────────────────────────────────────────

    [HttpGet("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> GetTwoFactorSetup(CancellationToken cancellationToken)
    {
        var twoFactorSetup = await authService.GetTwoFactorSetupAsync(CurrentUserId, cancellationToken);
        return twoFactorSetup is null ? NotFound() : Ok(twoFactorSetup);
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor(EnableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var enableTwoFactorResult = await authService.EnableTwoFactorAsync(CurrentUserId, request.Code, cancellationToken);
        return enableTwoFactorResult.Succeeded
            ? Ok(new { recoveryCodes = enableTwoFactorResult.Value })
            : ToErrorResponse(enableTwoFactorResult);
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(DisableTwoFactorRequest request, CancellationToken cancellationToken)
        => ToActionResult(await authService.DisableTwoFactorAsync(CurrentUserId, request.Code, cancellationToken));

    [HttpPost("2fa/verify")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyTwoFactor(TwoFactorLoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await authService.VerifyTwoFactorAsync(request.TwoFactorToken, request.Code, cancellationToken));

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Respuesta común de los inicios de sesión: error, "falta el código 2FA" o sesión completa.
    /// </summary>
    private IActionResult ToSignInResponse(AuthResult signInResult)
    {
        if (!signInResult.Succeeded)
        {
            return ToErrorResponse(signInResult);
        }

        if (signInResult.RequiresTwoFactor)
        {
            return Ok(new TwoFactorRequiredResponse { TwoFactorToken = signInResult.TwoFactorToken! });
        }

        return Ok(new AuthResponse
        {
            TokenType = BearerTokenType,
            AccessToken = signInResult.AccessToken!,
            ExpiresAtUtc = signInResult.AccessTokenExpiresAtUtc!.Value,
            RefreshToken = signInResult.RefreshToken!,
            RefreshTokenExpiresAtUtc = signInResult.RefreshTokenExpiresAtUtc!.Value,
            User = signInResult.User!
        });
    }
}
