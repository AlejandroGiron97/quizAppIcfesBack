using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Infrastructure.Content.SubscriptionOwnership;

/// <summary>Institutional user linked to an institution: covered by the institution's subscription.</summary>
public sealed class InstitutionalSubscriptionOwnerStrategy : ISubscriptionOwnerStrategy
{
    public bool AppliesTo(SubscriptionOwnerContext owner) =>
        owner.UserType == UserType.Institutional && owner.InstitutionId.HasValue;

    public IQueryable<Subscription> FilterOwnedSubscriptions(IQueryable<Subscription> subscriptions, SubscriptionOwnerContext owner) =>
        subscriptions.Where(subscription => subscription.InstitutionId == owner.InstitutionId);
}
