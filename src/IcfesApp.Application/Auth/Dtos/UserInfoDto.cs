namespace IcfesApp.Application.Auth.Dtos;

/// <summary>
/// Datos públicos del usuario autenticado. Se incluye en cada respuesta de login/refresh
/// para que el cliente no tenga que llamar a /session inmediatamente después.
/// </summary>
public class UserInfoDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>
    /// "Particular" o "Institutional". Determina si el usuario paga suscripción
    /// o si pertenece a una institución (no paga).
    /// </summary>
    public required string Type { get; init; }

    public Guid? SchoolId { get; init; }
}