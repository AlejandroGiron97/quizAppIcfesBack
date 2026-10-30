using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Infrastructure.Email.Messages;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IcfesApp.Infrastructure.Email;

/// <summary>
/// Sends the authentication emails: generates the Identity token, builds the message and sends it.
///
/// If sending fails it is only logged: the user should not see an error caused by a mail server
/// problem, and they can always ask for the link to be sent again.
/// </summary>
public class AuthEmailSender(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IOptions<EmailSettings> emailOptions,
    ILogger<AuthEmailSender> logger) : IAuthEmailSender
{
    public async Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationMessage = new EmailConfirmationMessage(
            user.FirstName, emailOptions.Value.ConfirmEmailUrlTemplate, user.Email!, emailConfirmationToken);

        await SendSafelyAsync(user.Email!, confirmationMessage, cancellationToken);
    }

    public async Task SendPasswordResetAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var passwordResetMessage = new PasswordResetMessage(
            user.FirstName, emailOptions.Value.ResetPasswordUrlTemplate, user.Email!, passwordResetToken);

        await SendSafelyAsync(user.Email!, passwordResetMessage, cancellationToken);
    }

    private async Task SendSafelyAsync(string recipientEmail, AuthEmailMessage emailMessage, CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(recipientEmail, emailMessage.Subject, emailMessage.BuildHtmlBody(), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Failed to send the \"{Subject}\" email to {Email}. The user can request a new link.",
                emailMessage.Subject, recipientEmail);
        }
    }
}
