using IcfesApp.Application.Subscriptions.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Subscriptions;

public static class SubscriptionMappings
{
    /// <remarks>Requires <see cref="Subscription.Plan"/> to be loaded (and Institution when it is institutional).</remarks>
    public static SubscriptionDto ToDto(this Subscription subscription)
    {
        var daysRemaining = (int)Math.Ceiling((subscription.ExpiresAtUtc - DateTime.UtcNow).TotalDays);

        return new SubscriptionDto
        {
            Id = subscription.Id,
            OwnerType = subscription.UserId.HasValue ? "Particular" : "Institutional",
            UserId = subscription.UserId,
            InstitutionId = subscription.InstitutionId,
            InstitutionName = subscription.Institution?.Name,
            PlanId = subscription.PlanId,
            PlanCode = subscription.Plan?.Code ?? string.Empty,
            PlanName = subscription.Plan?.Name ?? string.Empty,
            Status = subscription.Status.ToString(),
            StartedAtUtc = subscription.StartedAtUtc,
            ExpiresAtUtc = subscription.ExpiresAtUtc,
            CancelledAtUtc = subscription.CancelledAtUtc,
            AutoRenew = subscription.AutoRenew,
            IsCurrentlyActive = subscription.IsCurrentlyActive,
            DaysRemaining = daysRemaining
        };
    }
}
