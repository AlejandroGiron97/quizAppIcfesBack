using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Infrastructure.Content.SubscriptionOwnership;

/// <summary>
/// Particular user (or an institutional user with no institution assigned yet):
/// covered by their own subscription.
/// </summary>
public sealed class ParticularSubscriptionOwnerStrategy : ISubscriptionOwnerStrategy
{
    public bool AppliesTo(SubscriptionOwnerContext owner) =>
        !(owner.UserType == UserType.Institutional && owner.InstitutionId.HasValue);

    public IQueryable<Subscription> FilterOwnedSubscriptions(IQueryable<Subscription> subscriptions, SubscriptionOwnerContext owner) =>
        subscriptions.Where(subscription => subscription.UserId == owner.UserId);
}
