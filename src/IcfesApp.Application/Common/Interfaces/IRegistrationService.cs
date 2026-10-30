using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Models;

namespace IcfesApp.Application.Common.Interfaces;

/// <summary>Creation of new users.</summary>
public interface IRegistrationService
{
    /// <summary>
    /// Public student sign-up. With a valid invitation code the student is linked to the institution.
    /// Sends the confirmation email; the user cannot sign in until they confirm it.
    /// </summary>
    Task<AuthResult> RegisterStudentAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>An Admin creates a Teacher/Admin. Returns the created user (no tokens are issued for them).</summary>
    Task<Result<UserInfoDto>> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);
}
