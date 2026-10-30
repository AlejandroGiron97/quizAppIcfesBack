using IcfesApp.Application.Subscriptions.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISubscriptionService
{
    /// <summary>
    /// Returns the subscription that applies to the given user:
    /// - Particular: their own.
    /// - Institutional: their institution's.
    /// </summary>
    Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SubscriptionDto?> GetForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has active access (their own or through their institution).
    /// This is the check used to gate endpoints that require a subscription.
    /// </summary>
    Task<bool> HasActiveAccessAsync(Guid userId, CancellationToken cancellationToken = default);
}