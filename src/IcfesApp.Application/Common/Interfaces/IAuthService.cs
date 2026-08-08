using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<TwoFactorSetupDto?> GetTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<OperationResult> DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<AuthResult> VerifyTwoFactorAsync(string twoFactorToken, string code, CancellationToken cancellationToken = default);
    Task<OperationResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default);
}
