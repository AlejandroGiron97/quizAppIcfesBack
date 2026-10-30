using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>Two-factor authentication (2FA).</summary>
[Route("api/auth/2fa")]
public class TwoFactorController(ITwoFactorService twoFactorService) : SignInControllerBase
{
    [HttpGet("setup")]
    [Authorize]
    public async Task<IActionResult> GetSetup(CancellationToken cancellationToken)
    {
        var twoFactorSetup = await twoFactorService.GetSetupAsync(CurrentUserId, cancellationToken);
        return twoFactorSetup is null ? NotFound() : Ok(twoFactorSetup);
    }

    [HttpPost("enable")]
    [Authorize]
    public async Task<IActionResult> Enable(EnableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var enableResult = await twoFactorService.EnableAsync(CurrentUserId, request.Code, cancellationToken);
        return enableResult.Succeeded
            ? Ok(new { recoveryCodes = enableResult.Value })
            : ToErrorResponse(enableResult);
    }

    [HttpPost("disable")]
    [Authorize]
    public async Task<IActionResult> Disable(DisableTwoFactorRequest request, CancellationToken cancellationToken)
        => ToActionResult(await twoFactorService.DisableAsync(CurrentUserId, request.Code, cancellationToken));

    /// <summary>Second sign-in step when the user has 2FA enabled.</summary>
    [HttpPost("verify-login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyLogin(TwoFactorLoginRequest request, CancellationToken cancellationToken)
        => ToSignInResponse(await twoFactorService.VerifyLoginAsync(request.TwoFactorToken, request.Code, cancellationToken));
}
