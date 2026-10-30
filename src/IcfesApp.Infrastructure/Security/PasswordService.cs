using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Infrastructure.Email;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IcfesApp.Infrastructure.Security;

public class PasswordService(
    UserManager<ApplicationUser> userManager,
    IAuthTokenIssuer authTokenIssuer,
    IAuthEmailSender authEmailSender,
    ILogger<PasswordService> logger) : IPasswordService
{
    public async Task SendPasswordResetEmailAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return;
        }

        await authEmailSender.SendPasswordResetAsync(user, cancellationToken);
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
            return Result.Failed(resetPasswordResult.ErrorDescriptions());
        }

        await authTokenIssuer.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
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

        var isSameAsCurrentPassword = await userManager.CheckPasswordAsync(user, request.NewPassword);
        if (isSameAsCurrentPassword)
        {
            return Result.Failed(["La nueva contraseña debe ser diferente a la actual."]);
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return Result.Failed(changePasswordResult.ErrorDescriptions());
        }

        var revokedRefreshTokenCount = await authTokenIssuer.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        logger.LogInformation(
            "User {UserId} changed their password. {RevokedCount} refresh tokens revoked for security.",
            user.Id, revokedRefreshTokenCount);

        return Result.Success();
    }
}
