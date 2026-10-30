using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

/// <summary>Password recovery and password change. Changing it closes every open session.</summary>
public interface IPasswordService
{
    /// <summary>
    /// Sends the password reset email. It does not reveal whether the email exists
    /// (so registered emails cannot be discovered).
    /// </summary>
    Task SendPasswordResetEmailAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sets a new password using the token received by email.</summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>Password change for an authenticated user; requires the current password.</summary>
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
