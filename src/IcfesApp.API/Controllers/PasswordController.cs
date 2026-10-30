using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>Password recovery and password change.</summary>
[Route("api/auth")]
public class PasswordController(IPasswordService passwordService) : ApiControllerBase
{
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await passwordService.SendPasswordResetEmailAsync(request, cancellationToken);
        return Ok(new { message = "Si el correo existe, se enviarán instrucciones para restablecer la contraseña." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var resetPasswordResult = await passwordService.ResetPasswordAsync(request, cancellationToken);
        return resetPasswordResult.Succeeded
            ? Ok(new { message = "Contraseña restablecida correctamente. Ya puedes iniciar sesión." })
            : ToErrorResponse(resetPasswordResult);
    }

    /// <summary>
    /// Password change for an authenticated user.
    /// Requires the current password as a security measure.
    /// When it succeeds, every active refresh token of the user is revoked.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var changePasswordResult = await passwordService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return changePasswordResult.Succeeded
            ? Ok(new { message = "Contraseña actualizada correctamente. Por seguridad, deberás iniciar sesión de nuevo en otros dispositivos." })
            : ToErrorResponse(changePasswordResult);
    }
}
