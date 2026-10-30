using IcfesApp.Application.Auth;
using IcfesApp.Infrastructure.Identity;

namespace IcfesApp.Infrastructure.Security;

/// <summary>
/// Issues and revokes session tokens (JWT access token + refresh token stored in the database).
/// Shared by every authentication service that ends with "the user can now get in".
/// Lives in Infrastructure because it receives an Identity ApplicationUser.
/// </summary>
public interface IAuthTokenIssuer
{
    /// <summary>
    /// Common last step of every sign-in: if the user has 2FA enabled it returns
    /// "code required"; otherwise it issues the tokens.
    /// </summary>
    Task<AuthResult> CompleteSignInAsync(ApplicationUser user, CancellationToken cancellationToken);

    /// <summary>Creates a new access token and refresh token for the user. The result always includes the user data.</summary>
    Task<AuthResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken);

    /// <summary>Closes every session of the user. Returns how many refresh tokens were revoked.</summary>
    Task<int> RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
}
