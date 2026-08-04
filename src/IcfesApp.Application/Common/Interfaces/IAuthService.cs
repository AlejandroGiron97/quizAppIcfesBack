using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
