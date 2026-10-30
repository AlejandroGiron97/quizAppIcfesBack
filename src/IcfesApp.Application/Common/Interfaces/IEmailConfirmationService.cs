using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

/// <summary>Email confirmation for a newly registered user.</summary>
public interface IEmailConfirmationService
{
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends the confirmation link. It does not reveal whether the email exists or was already confirmed
    /// (so registered emails cannot be discovered).
    /// </summary>
    Task ResendConfirmationEmailAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);
}
