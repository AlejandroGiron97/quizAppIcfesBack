using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

/// <summary>Sign-in, session renewal and sign-out.</summary>
public interface ISessionService
{
    /// <summary>Email and password sign-in. May return "2FA code required".</summary>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Google sign-in. If the email does not exist, the user is created as a student.</summary>
    Task<AuthResult> LoginWithGoogleAsync(string googleIdToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a valid refresh token for a new one (and a new access token).
    /// Also returns the up-to-date user, so the app restores its session with a single request.
    /// </summary>
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Closes the session tied to a refresh token (logout).</summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
