using System.Security.Claims;
using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // ─────────────────────────────────────────────────────────────
    // Registro
    // ─────────────────────────────────────────────────────────────

    /// <summary>Registro público. Siempre crea un usuario con rol Student.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegistrationPendingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new RegistrationPendingResponse
        {
            Message = "Revisa tu correo para confirmar tu cuenta antes de iniciar sesión."
        });
    }

    /// <summary>Registro de staff (Teacher/Admin). Solo accesible por un Admin.</summary>
    [HttpPost("register-staff")]
    [Authorize(Policy = Policies.RequireAdmin)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterStaffAsync(request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(MapToAuthResponse(result));
    }

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
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (result.Succeeded)
        {
            if (result.RequiresTwoFactor)
            {
                return Ok(new TwoFactorRequiredResponse
                {
                    TwoFactorToken = result.TwoFactorToken!
                });
            }
            return Ok(MapToAuthResponse(result));
        }

        if (result.Errors.Any(e => e.Contains("confirmar tu correo", StringComparison.OrdinalIgnoreCase)))
            return StatusCode(StatusCodes.Status403Forbidden, new { errors = result.Errors });

        if (result.Errors.Any(e => e.Contains("bloqueada", StringComparison.OrdinalIgnoreCase)))
            return StatusCode(StatusCodes.Status423Locked, new { errors = result.Errors });

        return Unauthorized(new { errors = result.Errors });
    }

    /// <summary>
    /// Devuelve los datos públicos del usuario autenticado actual.
    /// Se usa al recargar la app para obtener estado fresco sin rotar tokens.
    /// </summary>
    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    public IActionResult Session()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new UserInfoDto
        {
            Id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Roles = roles
        });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (!result.Succeeded)
            return Unauthorized(new { errors = result.Errors });

        return Ok(new AuthSessionResponse
        {
            TokenType = "Bearer",
            AccessToken = result.AccessToken!,
            ExpiresAtUtc = result.AccessTokenExpiresAtUtc!.Value,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc!.Value
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
        var result = await authService.ResetPasswordAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(new { message = "Contraseña restablecida correctamente. Ya puedes iniciar sesión." })
            : BadRequest(new { errors = result.Errors });
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
        var result = await authService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return result.Succeeded
            ? Ok(new { message = "Contraseña actualizada correctamente. Por seguridad, deberás iniciar sesión de nuevo en otros dispositivos." })
            : BadRequest(new { errors = result.Errors });
    }

    // ─────────────────────────────────────────────────────────────
    // Confirmación de correo
    // ─────────────────────────────────────────────────────────────

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ConfirmEmailAsync(request, cancellationToken);
        return result.Succeeded
            ? Ok(new { message = "Correo confirmado correctamente. Ya puedes iniciar sesión." })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await authService.ResendConfirmationEmailAsync(request, cancellationToken);
        return Ok(new { message = "Si el correo existe y no ha sido confirmado, se enviará un nuevo enlace." });
    }

    // ─────────────────────────────────────────────────────────────
    // Google
    // ─────────────────────────────────────────────────────────────

    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.GoogleLoginAsync(request.IdToken, cancellationToken);

        if (result.Succeeded)
        {
            if (result.RequiresTwoFactor)
            {
                return Ok(new TwoFactorRequiredResponse
                {
                    TwoFactorToken = result.TwoFactorToken!
                });
            }
            return Ok(MapToAuthResponse(result));
        }

        return Unauthorized(new { errors = result.Errors });
    }

    // ─────────────────────────────────────────────────────────────
    // Doble factor (2FA)
    // ─────────────────────────────────────────────────────────────

    [HttpGet("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> GetTwoFactorSetup(CancellationToken cancellationToken)
    {
        var setup = await authService.GetTwoFactorSetupAsync(CurrentUserId, cancellationToken);
        return setup is null ? NotFound() : Ok(setup);
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor(EnableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.EnableTwoFactorAsync(CurrentUserId, request.Code, cancellationToken);
        return result.Succeeded
            ? Ok(new { recoveryCodes = result.Value })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(DisableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.DisableTwoFactorAsync(CurrentUserId, request.Code, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("2fa/verify")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyTwoFactor(TwoFactorLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.VerifyTwoFactorAsync(request.TwoFactorToken, request.Code, cancellationToken);
        return result.Succeeded
            ? Ok(MapToAuthResponse(result))
            : Unauthorized(new { errors = result.Errors });
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static AuthResponse MapToAuthResponse(AuthResult result) => new()
    {
        TokenType = "Bearer",
        AccessToken = result.AccessToken!,
        ExpiresAtUtc = result.AccessTokenExpiresAtUtc!.Value,
        RefreshToken = result.RefreshToken!,
        RefreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc!.Value,
        User = result.User!
    };
}