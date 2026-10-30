using IcfesApp.Domain.Enums;

namespace IcfesApp.Infrastructure.Content.SubscriptionOwnership;

/// <summary>The minimum we need to know about a user to find their subscription.</summary>
public sealed record SubscriptionOwnerContext(Guid UserId, UserType UserType, Guid? InstitutionId);
