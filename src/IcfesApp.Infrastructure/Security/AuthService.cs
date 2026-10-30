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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Infrastructure.Security;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IEmailSender emailSender,
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtOptions,
    IOptions<EmailSettings> emailOptions,
    IOptions<GoogleSettings> googleOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private const int RecoveryCodeCount = 10;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Si viene código de invitación, validarlo antes de crear el usuario.
        InvitationCode? invitationCode = null;
        if (!string.IsNullOrWhiteSpace(request.InvitationCode))
        {
            var normalizedCode = request.InvitationCode.Trim().ToUpperInvariant();
            invitationCode = await dbContext.InvitationCodes
                .FirstOrDefaultAsync(ic => ic.Code == normalizedCode, cancellationToken);

            if (invitationCode is null)
            {
                return AuthResult.Failed(["El código de invitación no es válido."]);
            }

            if (!invitationCode.IsUsable)
            {
                return AuthResult.Failed(["El código de invitación ya expiró o alcanzó su límite de usos."]);
            }
        }

        // 2. Crear el usuario con Type y SchoolId derivados del código (o Particular si no viene).
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Type = invitationCode is not null ? UserType.Institutional : UserType.Particular,
            SchoolId = invitationCode?.InstitutionId
        };

        // 3. Ejecutar en transacción: crear usuario + incrementar contador de usos del código.
        // Si algo falla, se revierte todo para no dejar estado inconsistente.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var createUserResult = await userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AuthResult.Failed(DescribeErrors(createUserResult));
            }

            await userManager.AddToRoleAsync(user, Roles.Student);

            if (invitationCode is not null)
            {
                invitationCode.Uses++;
                invitationCode.UpdatedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await SendConfirmationEmailAsync(user, cancellationToken);

        return AuthResult.RegistrationPending();
    }

    public async Task<Result<UserInfoDto>> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Role != Roles.Teacher && request.Role != Roles.Admin)
        {
            return Result<UserInfoDto>.Failed([$"El rol debe ser '{Roles.Teacher}' o '{Roles.Admin}'."]);
        }

        var staffUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var createUserResult = await userManager.CreateAsync(staffUser, request.Password);
        if (!createUserResult.Succeeded)
        {
            return Result<UserInfoDto>.Failed(DescribeErrors(createUserResult));
        }

        await userManager.AddToRoleAsync(staffUser, request.Role);
        return Result<UserInfoDto>.Success(BuildUserInfo(staffUser, [request.Role]));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        const string invalidCredentialsMessage = "Credenciales inválidas.";

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.Failed([invalidCredentialsMessage], ErrorType.Unauthorized);
        }

        var passwordCheckResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (passwordCheckResult.IsLockedOut)
        {
            return AuthResult.Failed(["Cuenta bloqueada temporalmente por múltiples intentos fallidos."], ErrorType.Locked);
        }

        if (!passwordCheckResult.Succeeded)
        {
            return AuthResult.Failed([invalidCredentialsMessage], ErrorType.Unauthorized);
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return AuthResult.Failed(
                ["Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada."],
                ErrorType.Forbidden);
        }

        return await CompleteSignInAsync(user, cancellationToken);
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return BuildUserInfo(user, roles);
    }

    public async Task<AuthResult> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload googlePayload;
        try
        {
            googlePayload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
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
            user = new ApplicationUser
            {
                UserName = googlePayload.Email,
                Email = googlePayload.Email,
                FirstName = string.IsNullOrWhiteSpace(googlePayload.GivenName) ? googlePayload.Email : googlePayload.GivenName,
                LastName = googlePayload.FamilyName ?? string.Empty,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                return AuthResult.Failed(DescribeErrors(createUserResult), ErrorType.Unauthorized);
            }

            await userManager.AddToRoleAsync(user, Roles.Student);
        }

        return await CompleteSignInAsync(user, cancellationToken);
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

        storedRefreshToken.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(user, cancellationToken, includeUser: false);
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
            return;
        }

        var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{emailOptions.Value.ResetPasswordUrlTemplate}?email={WebUtility.UrlEncode(request.Email)}&token={WebUtility.UrlEncode(passwordResetToken)}";

        var emailHtmlBody = $"""
            <p>Hola {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Recibimos una solicitud para restablecer tu contraseña en IcfesApp.</p>
            <p><a href="{resetLink}">Haz clic aquí para crear una nueva contraseña</a></p>
            <p>Si no fuiste tú quien lo solicitó, puedes ignorar este correo.</p>
            """;

        try
        {
            await emailSender.SendAsync(request.Email, "Recupera tu contraseña - IcfesApp", emailHtmlBody, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Falló el envío del correo de recuperación para {Email}. El usuario puede solicitar un nuevo enlace.",
                request.Email);
        }
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failed(["Token o usuario inválido."]);
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return Result.Failed(DescribeErrors(resetPasswordResult));
        }

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failed(["Usuario no encontrado."]);
        }

        var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isCurrentPasswordValid)
        {
            return Result.Failed(["La contraseña actual es incorrecta."]);
        }

        var isSamePassword = await userManager.CheckPasswordAsync(user, request.NewPassword);
        if (isSamePassword)
        {
            return Result.Failed(["La nueva contraseña debe ser diferente a la actual."]);
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return Result.Failed(DescribeErrors(changePasswordResult));
        }

        var revokedRefreshTokenCount = await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        logger.LogInformation(
            "Usuario {UserId} cambió su contraseña. {RevokedCount} refresh tokens revocados por seguridad.",
            user.Id, revokedRefreshTokenCount);

        return Result.Success();
    }

    public async Task<TwoFactorSetupDto?> GetTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(authenticatorKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var encodedIssuer = Uri.EscapeDataString("IcfesApp");
        var encodedEmail = Uri.EscapeDataString(user.Email!);
        var authenticatorUri = $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={authenticatorKey}&issuer={encodedIssuer}&digits=6";

        return new TwoFactorSetupDto { SharedKey = FormatKey(authenticatorKey!), AuthenticatorUri = authenticatorUri };
    }

    public async Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<IReadOnlyList<string>>.Failed(["Usuario no encontrado."]);
        }

        var isAuthenticatorCodeValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isAuthenticatorCodeValid)
        {
            return Result<IReadOnlyList<string>>.Failed(["Código inválido."]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);

        return Result<IReadOnlyList<string>>.Success(recoveryCodes?.ToList() ?? []);
    }

    public async Task<Result> DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failed(["Usuario no encontrado."]);
        }

        var isAuthenticatorCodeValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isAuthenticatorCodeValid)
        {
            return Result.Failed(["Código inválido."]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);

        return Result.Success();
    }

    public async Task<AuthResult> VerifyTwoFactorAsync(string twoFactorToken, string code, CancellationToken cancellationToken = default)
    {
        var userId = jwtTokenService.ValidateTwoFactorChallengeToken(twoFactorToken);
        if (userId is null)
        {
            return AuthResult.Failed(["Token de doble factor inválido o expirado."], ErrorType.Unauthorized);
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return AuthResult.Failed(["Usuario no encontrado."], ErrorType.Unauthorized);
        }

        var isAuthenticatorCodeValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isAuthenticatorCodeValid)
        {
            var redeemRecoveryCodeResult = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
            if (!redeemRecoveryCodeResult.Succeeded)
            {
                return AuthResult.Failed(["Código inválido."], ErrorType.Unauthorized);
            }
        }

        return await IssueTokensAsync(user, cancellationToken, includeUser: true);
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failed(["Token o usuario inválido."]);
        }

        var confirmEmailResult = await userManager.ConfirmEmailAsync(user, request.Token);
        return confirmEmailResult.Succeeded
            ? Result.Success()
            : Result.Failed(DescribeErrors(confirmEmailResult));
    }

    public async Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            return;
        }

        try
        {
            await SendConfirmationEmailAsync(user, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falló el reenvío de confirmación para {Email}.", user.Email);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Último paso común de todos los inicios de sesión (correo/contraseña y Google):
    /// si el usuario tiene 2FA se pide el código; si no, se emiten los tokens.
    /// </summary>
    private async Task<AuthResult> CompleteSignInAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var twoFactorChallengeToken = jwtTokenService.CreateTwoFactorChallengeToken(user.Id);
            return AuthResult.TwoFactorRequired(twoFactorChallengeToken);
        }

        return await IssueTokensAsync(user, cancellationToken, includeUser: true);
    }

    private async Task<int> RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
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

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmLink = $"{emailOptions.Value.ConfirmEmailUrlTemplate}?email={WebUtility.UrlEncode(user.Email)}&token={WebUtility.UrlEncode(emailConfirmationToken)}";

        var emailHtmlBody = $"""
            <p>Hola {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Gracias por registrarte en IcfesApp. Confirma tu correo para activar tu cuenta:</p>
            <p><a href="{confirmLink}">Confirmar mi correo</a></p>
            <p>Si no fuiste tú quien se registró, puedes ignorar este correo.</p>
            """;

        try
        {
            await emailSender.SendAsync(user.Email!, "Confirma tu correo - IcfesApp", emailHtmlBody, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Falló el envío del correo de confirmación para {Email}. El usuario fue creado; se puede reintentar con /api/auth/resend-confirmation.",
                user.Email);
        }
    }

    /// <summary>Separa la clave en grupos de 4 caracteres para que sea fácil de copiar a mano.</summary>
    private static string FormatKey(string unformattedKey)
    {
        const int groupLength = 4;
        var formattedKey = new System.Text.StringBuilder();
        var position = 0;
        while (position + groupLength < unformattedKey.Length)
        {
            formattedKey.Append(unformattedKey.AsSpan(position, groupLength)).Append(' ');
            position += groupLength;
        }

        if (position < unformattedKey.Length)
        {
            formattedKey.Append(unformattedKey.AsSpan(position));
        }

        return formattedKey.ToString();
    }

    private static IEnumerable<string> DescribeErrors(IdentityResult identityResult) =>
        identityResult.Errors.Select(identityError => identityError.Description);

    private static UserInfoDto BuildUserInfo(ApplicationUser user, IEnumerable<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = roles.ToList(),
        Type = user.Type.ToString(),
        SchoolId = user.SchoolId
    };

    private async Task<AuthResult> IssueTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        bool includeUser)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiresAtUtc) = jwtTokenService.CreateAccessToken(
            user.Id, user.Email!, roles, user.Type);
        var refreshTokenValue = jwtTokenService.CreateRefreshToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var userInfo = includeUser ? BuildUserInfo(user, roles) : null;

        return AuthResult.Success(accessToken, accessTokenExpiresAtUtc, refreshTokenValue, refreshTokenExpiresAtUtc, userInfo);
    }
}