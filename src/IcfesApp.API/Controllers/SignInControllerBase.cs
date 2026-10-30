using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>
/// Base class for the controllers that finish a sign-in (AuthController and TwoFactorController).
/// Hierarchy: ControllerBase → ApiControllerBase → SignInControllerBase → AuthController / TwoFactorController.
/// </summary>
public abstract class SignInControllerBase : ApiControllerBase
{
    protected const string BearerTokenType = "Bearer";

    /// <summary>
    /// Common sign-in response: an error, "2FA code required", or a full session with tokens.
    /// </summary>
    protected IActionResult ToSignInResponse(AuthResult signInResult)
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
