using IcfesApp.Infrastructure.Identity;

namespace IcfesApp.Infrastructure.Email;

/// <summary>
/// Authentication emails (confirm email, recover password).
/// Lives in Infrastructure instead of Application because it receives an Identity ApplicationUser.
/// </summary>
public interface IAuthEmailSender
{
    Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(ApplicationUser user, CancellationToken cancellationToken);
}
