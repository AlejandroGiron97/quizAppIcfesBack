using Google.Apis.Auth;
using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IcfesApp.Infrastructure.Security;

public class SessionService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext dbContext,
    IAuthTokenIssuer authTokenIssuer,
    IOptions<GoogleSettings> googleOptions) : ISessionService
{
    private const string InvalidCredentialsMessage = "Credenciales inválidas.";

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.Failed([InvalidCredentialsMessage], ErrorType.Unauthorized);
        }

        var passwordCheckResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (passwordCheckResult.IsLockedOut)
        {
            return AuthResult.Failed(["Cuenta bloqueada temporalmente por múltiples intentos fallidos."], ErrorType.Locked);
        }

        if (!passwordCheckResult.Succeeded)
        {
            return AuthResult.Failed([InvalidCredentialsMessage], ErrorType.Unauthorized);
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return AuthResult.Failed(
                ["Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada."],
                ErrorType.Forbidden);
        }

        return await authTokenIssuer.CompleteSignInAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginWithGoogleAsync(string googleIdToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload googlePayload;
        try
        {
            googlePayload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleOptions.Value.ClientId]
            });
        }
        catch (InvalidJwtException)
        {
            return AuthResult.Failed(["Token de Google inválido."], ErrorType.Unauthorized);
        }

        if (!googlePayload.EmailVerified)
        {
            return AuthResult.Failed(["El correo de la cuenta de Google no está verificado."], ErrorType.Unauthorized);
        }

        var user = await userManager.FindByEmailAsync(googlePayload.Email);
        if (user is null)
        {
            user = ApplicationUserFactory.CreateFromGoogle(googlePayload.Email, googlePayload.GivenName, googlePayload.FamilyName);

            var createUserResult = await userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                return AuthResult.Failed(createUserResult.ErrorDescriptions(), ErrorType.Unauthorized);
            }

            await userManager.AddToRoleAsync(user, Roles.Student);
        }

        return await authTokenIssuer.CompleteSignInAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive)
        {
            return AuthResult.Failed(["Refresh token inválido o expirado."], ErrorType.Unauthorized);
        }

        var user = await userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());
        if (user is null)
        {
            return AuthResult.Failed(["Usuario no encontrado."], ErrorType.Unauthorized);
        }

        // Rotation: the used refresh token is revoked and a new one is issued (both are saved together).
        // The response includes the user too, so the app can restore its session with this single request.
        storedRefreshToken.RevokedAtUtc = DateTime.UtcNow;
        return await authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.Token == refreshToken, cancellationToken);

        if (storedRefreshToken is not null && storedRefreshToken.IsActive)
        {
            storedRefreshToken.RevokedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
