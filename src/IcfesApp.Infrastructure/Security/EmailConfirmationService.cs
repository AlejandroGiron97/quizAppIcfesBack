using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Infrastructure.Email;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Security;

public class EmailConfirmationService(
    UserManager<ApplicationUser> userManager,
    IAuthEmailSender authEmailSender) : IEmailConfirmationService
{
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
            : Result.Failed(confirmEmailResult.ErrorDescriptions());
    }

    public async Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            return;
        }

        await authEmailSender.SendEmailConfirmationAsync(user, cancellationToken);
    }
}
