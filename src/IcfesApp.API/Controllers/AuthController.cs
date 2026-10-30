using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>
/// Registration and session. The other /api/auth routes live in:
/// PasswordController, EmailConfirmationController and TwoFactorController.
/// </summary>
[Route("api/auth")]
public class AuthController(
    IRegistrationService registrationService,
    ISessionService sessionService) : SignInControllerBase
{
    // ─────────────────────────────────────────────────────────────
    // Registration
    // ─────────────────────────────────────────────────────────────

    /// <summary>Public sign-up. Always creates a user with the Student role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegistrationPendingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var registrationResult = await registrationService.RegisterStudentAsync(request, cancellationToken);
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
    /// Staff registration (Teacher/Admin). Only an Admin can call it.
    /// Returns the created user; it does not issue tokens for them (the admin keeps their own session).
    /// </summary>
    [HttpPost("register-staff")]
    [Authorize(Policy = Policies.RequireAdmin)]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request, CancellationToken cancellationToken)
        => ToActionResult(await registrationService.RegisterStaffAsync(request, cancellationToken));

    // ─────────────────────────────────────────────────────────────
    // Session
    // ─────────────────────────────────────────────────────────────

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await sessionService.LoginAsync(request, cancellationToken));

    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TwoFactorRequiredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await sessionService.LoginWithGoogleAsync(request.IdToken, cancellationToken));

    /// <summary>
    /// Returns the up-to-date data of the authenticated user (read from the database).
    /// Used when the app reloads to get fresh state without rotating tokens.
    /// </summary>
    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        var currentUser = await sessionService.GetCurrentUserAsync(CurrentUserId, cancellationToken);
        return currentUser is null ? Unauthorized() : Ok(currentUser);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var refreshResult = await sessionService.RefreshAsync(request.RefreshToken, cancellationToken);
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
        await sessionService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }
}
