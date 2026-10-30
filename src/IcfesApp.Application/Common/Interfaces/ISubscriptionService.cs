using IcfesApp.Application.Subscriptions.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISubscriptionService
{
    /// <summary>
    /// Returns the subscription that applies to the given user:
    /// - Particular: their own.
    /// - Institutional: their institution's.
    /// </summary>
    Task<SubscriptionDto?> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Every subscription, newest first. Optionally only the ones of one institution.</summary>
    Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(Guid? institutionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has active access (their own or through their institution).
    /// This is the check used to gate endpoints that require a subscription.
    /// </summary>
    Task<bool> HasActiveAccessAsync(Guid userId, CancellationToken cancellationToken = default);
}
