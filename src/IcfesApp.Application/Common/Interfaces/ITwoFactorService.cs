using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

/// <summary>Two-factor authentication (2FA) with an authenticator app (TOTP).</summary>
public interface ITwoFactorService
{
    /// <summary>Key and URI (for the QR code) that the user registers in their authenticator app.</summary>
    Task<TwoFactorSetupDto?> GetSetupAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Enables 2FA if the code is valid. Returns the recovery codes.</summary>
    Task<Result<IReadOnlyList<string>>> EnableAsync(Guid userId, string authenticatorCode, CancellationToken cancellationToken = default);

    Task<Result> DisableAsync(Guid userId, string authenticatorCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Second sign-in step: validates the code (or a recovery code) and issues the tokens.
    /// </summary>
    Task<AuthResult> VerifyLoginAsync(string twoFactorChallengeToken, string authenticatorCode, CancellationToken cancellationToken = default);
}
