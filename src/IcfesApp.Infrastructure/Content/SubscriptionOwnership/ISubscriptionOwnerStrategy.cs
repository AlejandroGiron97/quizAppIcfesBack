using IcfesApp.Domain.Entities;

namespace IcfesApp.Infrastructure.Content.SubscriptionOwnership;

/// <summary>
/// Decides whose subscription grants access to a user (Strategy pattern).
///
/// There is one strategy per owner type:
///   - <see cref="ParticularSubscriptionOwnerStrategy"/>: the subscription belongs to the user.
///   - <see cref="InstitutionalSubscriptionOwnerStrategy"/>: the subscription belongs to their institution.
///
/// The <see cref="AppliesTo"/> conditions do not overlap: exactly one applies to each user.
/// For a new owner type (e.g. "family"), create another class and register it in DependencyInjection.
/// </summary>
public interface ISubscriptionOwnerStrategy
{
    bool AppliesTo(SubscriptionOwnerContext owner);

    /// <summary>Filters the subscriptions that belong to that owner.</summary>
    IQueryable<Subscription> FilterOwnedSubscriptions(IQueryable<Subscription> subscriptions, SubscriptionOwnerContext owner);
}
