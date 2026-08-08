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
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("register-staff")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterStaffAsync(request, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(RefreshRequest request, CancellationToken cancellationToken)
    {
        await authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

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
            ? Ok(new { message = "Contraseña actualizada correctamente." })
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.GoogleLoginAsync(request.IdToken, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : Unauthorized(new { errors = result.Errors });
    }

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
    public async Task<IActionResult> VerifyTwoFactor(TwoFactorLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.VerifyTwoFactorAsync(request.TwoFactorToken, request.Code, cancellationToken);
        return result.Succeeded ? Ok(ToResponse(result)) : Unauthorized(new { errors = result.Errors });
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static object ToResponse(AuthResult result)
    {
        if (result.RequiresEmailConfirmation)
        {
            return new { requiresEmailConfirmation = true, message = "Revisa tu correo para confirmar tu cuenta antes de iniciar sesión." };
        }

        if (result.RequiresTwoFactor)
        {
            return new { requiresTwoFactor = true, twoFactorToken = result.TwoFactorToken };
        }

        return new
        {
            tokenType = "Bearer",
            accessToken = result.AccessToken,
            expiresAtUtc = result.AccessTokenExpiresAtUtc,
            refreshToken = result.RefreshToken,
            refreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
            roles = result.Roles
        };
    }
}
