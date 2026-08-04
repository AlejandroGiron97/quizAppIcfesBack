using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IcfesApp.Infrastructure.Security;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return AuthResult.Failed(result.Errors.Select(e => e.Description));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.Failed(["Credenciales inválidas."]);
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return AuthResult.Failed(signInResult.IsLockedOut
                ? ["Cuenta bloqueada temporalmente por múltiples intentos fallidos."]
                : ["Credenciales inválidas."]);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return AuthResult.Failed(["Refresh token inválido o expirado."]);
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return AuthResult.Failed(["Usuario no encontrado."]);
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is not null && stored.IsActive)
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessExpiresAtUtc) = jwtTokenService.CreateAccessToken(user.Id, user.Email!, roles);
        var refreshTokenValue = jwtTokenService.CreateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAtUtc = refreshExpiresAtUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(accessToken, accessExpiresAtUtc, refreshTokenValue, refreshExpiresAtUtc, roles);
    }
}
