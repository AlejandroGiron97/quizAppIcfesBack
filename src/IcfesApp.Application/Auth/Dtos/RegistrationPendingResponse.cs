namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Response of /api/auth/register when sign-up succeeded but the user must
/// confirm their email before they can sign in.
/// </summary>
public class RegistrationPendingResponse
{
    public bool RequiresEmailConfirmation { get; init; } = true;
    public required string Message { get; init; }
}