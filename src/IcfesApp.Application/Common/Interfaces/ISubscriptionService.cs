using IcfesApp.Application.Subscriptions.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISubscriptionService
{
    /// <summary>
    /// Devuelve la suscripción relevante para el usuario indicado:
    /// - Si es Particular: la suya propia.
    /// - Si es Institutional: la de su institución.
    /// </summary>
    Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SubscriptionDto?> GetForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve true si el usuario tiene acceso activo (propio o por su institución).
    /// Es la función que se usará para gatear endpoints que requieren suscripción.
    /// </summary>
    Task<bool> HasActiveAccessAsync(Guid userId, CancellationToken cancellationToken = default);
}