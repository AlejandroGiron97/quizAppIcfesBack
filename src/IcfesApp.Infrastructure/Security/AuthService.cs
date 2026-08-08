using System.Net;
using Google.Apis.Auth;
using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Email;
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
    IEmailSender emailSender,
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtOptions,
    IOptions<EmailSettings> emailOptions,
    IOptions<GoogleSettings> googleOptions) : IAuthService
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

        await userManager.AddToRoleAsync(user, Roles.Student);
        await SendConfirmationEmailAsync(user, cancellationToken);

        return AuthResult.RegistrationPending();
    }

    public async Task<AuthResult> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Role != Roles.Teacher && request.Role != Roles.Admin)
        {
            return AuthResult.Failed([$"El rol debe ser '{Roles.Teacher}' o '{Roles.Admin}'."]);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            // Provisionado por un Admin: ya se vouch por la cuenta, no necesita confirmar el correo.
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return AuthResult.Failed(result.Errors.Select(e => e.Description));
        }

        await userManager.AddToRoleAsync(user, request.Role);
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

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return AuthResult.Failed(["Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada."]);
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var challengeToken = jwtTokenService.CreateTwoFactorChallengeToken(user.Id);
            return AuthResult.TwoFactorRequired(challengeToken);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleOptions.Value.ClientId]
            });
        }
        catch (InvalidJwtException)
        {
            return AuthResult.Failed(["Token de Google inválido."]);
        }

        if (!payload.EmailVerified)
        {
            return AuthResult.Failed(["El correo de la cuenta de Google no está verificado."]);
        }

        var user = await userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                FirstName = string.IsNullOrWhiteSpace(payload.GivenName) ? payload.Email : payload.GivenName,
                LastName = payload.FamilyName ?? string.Empty,
                // Google ya verificó el correo, no hace falta que confirme de nuevo.
                EmailConfirmed = true
            };

            // Sin password: esta cuenta solo puede entrar por Google, a menos que más adelante
            // se agregue una forma de establecer contraseña.
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return AuthResult.Failed(createResult.Errors.Select(e => e.Description));
            }

            await userManager.AddToRoleAsync(user, Roles.Student);
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var challengeToken = jwtTokenService.CreateTwoFactorChallengeToken(user.Id);
            return AuthResult.TwoFactorRequired(challengeToken);
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

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // No revelamos si el email existe o no: simplemente no se envía nada.
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{emailOptions.Value.ResetPasswordUrlTemplate}?email={WebUtility.UrlEncode(request.Email)}&token={WebUtility.UrlEncode(token)}";

        var body = $"""
            <p>Hola {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Recibimos una solicitud para restablecer tu contraseña en IcfesApp.</p>
            <p><a href="{resetLink}">Haz clic aquí para crear una nueva contraseña</a></p>
            <p>Si no fuiste tú quien lo solicitó, puedes ignorar este correo.</p>
            """;

        await emailSender.SendAsync(request.Email, "Recupera tu contraseña - IcfesApp", body, cancellationToken);
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return OperationResult.Failed(["Token o usuario inválido."]);
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failed(result.Errors.Select(e => e.Description));
    }

    public async Task<TwoFactorSetupDto?> GetTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var issuer = Uri.EscapeDataString("IcfesApp");
        var email = Uri.EscapeDataString(user.Email!);
        var authenticatorUri = $"otpauth://totp/{issuer}:{email}?secret={key}&issuer={issuer}&digits=6";

        return new TwoFactorSetupDto { SharedKey = FormatKey(key!), AuthenticatorUri = authenticatorUri };
    }

    public async Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<IReadOnlyList<string>>.Failed(["Usuario no encontrado."]);
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isValid)
        {
            return Result<IReadOnlyList<string>>.Failed(["Código inválido."]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Result<IReadOnlyList<string>>.Success(recoveryCodes?.ToList() ?? []);
    }

    public async Task<OperationResult> DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult.Failed(["Usuario no encontrado."]);
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isValid)
        {
            return OperationResult.Failed(["Código inválido."]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        // Invalida la clave vieja: si se vuelve a activar, requiere escanear un QR nuevo.
        await userManager.ResetAuthenticatorKeyAsync(user);

        return OperationResult.Success();
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string twoFactorToken, string code, CancellationToken cancellationToken = default)
    {
        var userId = jwtTokenService.ValidateTwoFactorChallengeToken(twoFactorToken);
        if (userId is null)
        {
            return AuthResult.Failed(["Token de doble factor inválido o expirado."]);
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return AuthResult.Failed(["Usuario no encontrado."]);
        }

        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isValidCode)
        {
            var recoveryResult = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
            if (!recoveryResult.Succeeded)
            {
                return AuthResult.Failed(["Código inválido."]);
            }
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<OperationResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return OperationResult.Failed(["Token o usuario inválido."]);
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failed(result.Errors.Select(e => e.Description));
    }

    public async Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            // No revelamos si el email existe, ni si ya estaba confirmado.
            return;
        }

        await SendConfirmationEmailAsync(user, cancellationToken);
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmLink = $"{emailOptions.Value.ConfirmEmailUrlTemplate}?email={WebUtility.UrlEncode(user.Email)}&token={WebUtility.UrlEncode(token)}";

        var body = $"""
            <p>Hola {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Gracias por registrarte en IcfesApp. Confirma tu correo para activar tu cuenta:</p>
            <p><a href="{confirmLink}">Confirmar mi correo</a></p>
            <p>Si no fuiste tú quien se registró, puedes ignorar este correo.</p>
            """;

        await emailSender.SendAsync(user.Email!, "Confirma tu correo - IcfesApp", body, cancellationToken);
    }

    private static string FormatKey(string unformattedKey)
    {
        var formatted = new System.Text.StringBuilder();
        var position = 0;
        while (position + 4 < unformattedKey.Length)
        {
            formatted.Append(unformattedKey.AsSpan(position, 4)).Append(' ');
            position += 4;
        }

        if (position < unformattedKey.Length)
        {
            formatted.Append(unformattedKey.AsSpan(position));
        }

        return formatted.ToString();
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
