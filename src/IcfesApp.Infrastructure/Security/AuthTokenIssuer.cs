using IcfesApp.Application.Auth;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IcfesApp.Infrastructure.Security;

public class AuthTokenIssuer(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtOptions) : IAuthTokenIssuer
{
    public async Task<AuthResult> CompleteSignInAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var twoFactorChallengeToken = jwtTokenService.CreateTwoFactorChallengeToken(user.Id);
            return AuthResult.TwoFactorRequired(twoFactorChallengeToken);
        }

        return await IssueTokensAsync(user, includeUser: true, cancellationToken);
    }

    public async Task<AuthResult> IssueTokensAsync(ApplicationUser user, bool includeUser, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiresAtUtc) = jwtTokenService.CreateAccessToken(user.Id, user.Email!, roles, user.Type);
        var refreshTokenValue = jwtTokenService.CreateRefreshToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var userInfo = includeUser ? user.ToUserInfoDto(roles) : null;

        return AuthResult.Success(accessToken, accessTokenExpiresAtUtc, refreshTokenValue, refreshTokenExpiresAtUtc, userInfo);
    }

    public async Task<int> RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeRefreshTokens = await dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.UserId == userId && refreshToken.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        if (activeRefreshTokens.Count == 0)
        {
            return 0;
        }

        var revokedAtUtc = DateTime.UtcNow;
        foreach (var activeRefreshToken in activeRefreshTokens)
        {
            activeRefreshToken.RevokedAtUtc = revokedAtUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return activeRefreshTokens.Count;
    }
}
