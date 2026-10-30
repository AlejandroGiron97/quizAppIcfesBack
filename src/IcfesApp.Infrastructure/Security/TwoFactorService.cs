using System.Text;
using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Security;

public class TwoFactorService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IAuthTokenIssuer authTokenIssuer) : ITwoFactorService
{
    private const string AuthenticatorIssuer = "IcfesApp";
    private const int RecoveryCodeCount = 10;
    private const int KeyGroupLength = 4;
    private const string InvalidCodeMessage = "Código inválido.";
    private const string UserNotFoundMessage = "Usuario no encontrado.";

    public async Task<TwoFactorSetupDto?> GetSetupAsync(Guid userId, CancellationToken cancellationToken = default)
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

        var encodedIssuer = Uri.EscapeDataString(AuthenticatorIssuer);
        var encodedEmail = Uri.EscapeDataString(user.Email!);
        var authenticatorUri = $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={authenticatorKey}&issuer={encodedIssuer}&digits=6";

        return new TwoFactorSetupDto { SharedKey = FormatKeyInGroups(authenticatorKey!), AuthenticatorUri = authenticatorUri };
    }

    public async Task<Result<IReadOnlyList<string>>> EnableAsync(Guid userId, string authenticatorCode, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<IReadOnlyList<string>>.Failed([UserNotFoundMessage]);
        }

        if (!await IsAuthenticatorCodeValidAsync(user, authenticatorCode))
        {
            return Result<IReadOnlyList<string>>.Failed([InvalidCodeMessage]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);

        return Result<IReadOnlyList<string>>.Success(recoveryCodes?.ToList() ?? []);
    }

    public async Task<Result> DisableAsync(Guid userId, string authenticatorCode, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failed([UserNotFoundMessage]);
        }

        if (!await IsAuthenticatorCodeValidAsync(user, authenticatorCode))
        {
            return Result.Failed([InvalidCodeMessage]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);

        return Result.Success();
    }

    public async Task<AuthResult> VerifyLoginAsync(string twoFactorChallengeToken, string authenticatorCode, CancellationToken cancellationToken = default)
    {
        var userId = jwtTokenService.ValidateTwoFactorChallengeToken(twoFactorChallengeToken);
        if (userId is null)
        {
            return AuthResult.Failed(["Token de doble factor inválido o expirado."], ErrorType.Unauthorized);
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return AuthResult.Failed([UserNotFoundMessage], ErrorType.Unauthorized);
        }

        // Accept the authenticator app code or, if the user lost their phone, one of their recovery codes.
        var isCodeAccepted = await IsAuthenticatorCodeValidAsync(user, authenticatorCode)
            || (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, authenticatorCode)).Succeeded;

        if (!isCodeAccepted)
        {
            return AuthResult.Failed([InvalidCodeMessage], ErrorType.Unauthorized);
        }

        return await authTokenIssuer.IssueTokensAsync(user, includeUser: true, cancellationToken);
    }

    private Task<bool> IsAuthenticatorCodeValidAsync(ApplicationUser user, string authenticatorCode) =>
        userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, authenticatorCode);

    /// <summary>Splits the key into groups of 4 characters so it is easy to copy by hand.</summary>
    private static string FormatKeyInGroups(string unformattedKey)
    {
        var formattedKey = new StringBuilder();
        var position = 0;
        while (position + KeyGroupLength < unformattedKey.Length)
        {
            formattedKey.Append(unformattedKey.AsSpan(position, KeyGroupLength)).Append(' ');
            position += KeyGroupLength;
        }

        if (position < unformattedKey.Length)
        {
            formattedKey.Append(unformattedKey.AsSpan(position));
        }

        return formattedKey.ToString();
    }
}
