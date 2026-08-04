using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
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

    private static object ToResponse(AuthResult result) => new
    {
        tokenType = "Bearer",
        accessToken = result.AccessToken,
        expiresAtUtc = result.AccessTokenExpiresAtUtc,
        refreshToken = result.RefreshToken,
        refreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
        roles = result.Roles
    };
}
