using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Subscriptions;
using IcfesApp.Application.Subscriptions.Dtos;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Content.SubscriptionOwnership;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

/// <summary>
/// Uses ApplicationDbContext (not IApplicationDbContext) because it needs to read the users table,
/// which belongs to Identity and lives in Infrastructure.
///
/// "Whose subscription applies to this user" is decided by a strategy
/// (<see cref="ISubscriptionOwnerStrategy"/>); this service has no ifs per user type.
/// </summary>
public class SubscriptionService(
    ApplicationDbContext dbContext,
    IEnumerable<ISubscriptionOwnerStrategy> ownerStrategies) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var owner = await FindOwnerContextAsync(userId, cancellationToken);
        if (owner is null)
        {
            return null;
        }

        var subscriptionsWithDetails = dbContext.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Plan)
            .Include(subscription => subscription.Institution);

        var latestOwnedSubscription = await SelectStrategyFor(owner)
            .FilterOwnedSubscriptions(subscriptionsWithDetails, owner)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return latestOwnedSubscription?.ToDto();
    }

    public async Task<SubscriptionDto?> GetForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var latestInstitutionSubscription = await dbContext.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Plan)
            .Include(subscription => subscription.Institution)
            .Where(subscription => subscription.InstitutionId == institutionId)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return latestInstitutionSubscription?.ToDto();
    }

    public async Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await dbContext.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Plan)
            .Include(subscription => subscription.Institution)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(subscription => subscription.ToDto()).ToList();
    }

    public async Task<bool> HasActiveAccessAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var owner = await FindOwnerContextAsync(userId, cancellationToken);
        if (owner is null)
        {
            return false;
        }

        var nowUtc = DateTime.UtcNow;
        IQueryable<Subscription> activeSubscriptions = dbContext.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Active && subscription.ExpiresAtUtc > nowUtc);

        return await SelectStrategyFor(owner)
            .FilterOwnedSubscriptions(activeSubscriptions, owner)
            .AnyAsync(cancellationToken);
    }

    private ISubscriptionOwnerStrategy SelectStrategyFor(SubscriptionOwnerContext owner) =>
        ownerStrategies.Single(ownerStrategy => ownerStrategy.AppliesTo(owner));

    /// <summary>Reads only what is needed from the user: their type and their institution.</summary>
    private Task<SubscriptionOwnerContext?> FindOwnerContextAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users
            .Where(user => user.Id == userId)
            .Select(user => new SubscriptionOwnerContext(user.Id, user.Type, user.SchoolId))
            .FirstOrDefaultAsync(cancellationToken);
}
