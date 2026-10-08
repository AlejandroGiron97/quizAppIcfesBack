namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Respuesta de /api/auth/register cuando el registro se completó pero requiere
/// que el usuario confirme su correo antes de poder iniciar sesión.
/// </summary>
public class RegistrationPendingResponse
{
    public bool RequiresEmailConfirmation { get; init; } = true;
    public required string Message { get; init; }
}