using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Subscriptions;
using IcfesApp.Application.Subscriptions.Dtos;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

/// <summary>
/// Usa ApplicationDbContext (y no IApplicationDbContext) porque necesita leer la tabla de usuarios,
/// que pertenece a Identity y vive en Infrastructure.
/// </summary>
public class SubscriptionService(ApplicationDbContext dbContext) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userAccess = await FindUserAccessAsync(userId, cancellationToken);
        if (userAccess is null)
        {
            return null;
        }

        if (userAccess.Type == UserType.Institutional && userAccess.SchoolId.HasValue)
        {
            return await GetForInstitutionAsync(userAccess.SchoolId.Value, cancellationToken);
        }

        var latestUserSubscription = await dbContext.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Plan)
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return latestUserSubscription?.ToDto();
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
        var userAccess = await FindUserAccessAsync(userId, cancellationToken);
        if (userAccess is null)
        {
            return false;
        }

        var nowUtc = DateTime.UtcNow;
        var activeSubscriptions = dbContext.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Active && subscription.ExpiresAtUtc > nowUtc);

        return userAccess.Type switch
        {
            UserType.Particular => await activeSubscriptions
                .AnyAsync(subscription => subscription.UserId == userId, cancellationToken),

            UserType.Institutional when userAccess.SchoolId.HasValue => await activeSubscriptions
                .AnyAsync(subscription => subscription.InstitutionId == userAccess.SchoolId.Value, cancellationToken),

            _ => false
        };
    }

    /// <summary>Lee solo lo necesario para decidir de quién es la suscripción: el tipo de usuario y su institución.</summary>
    private Task<UserAccessInfo?> FindUserAccessAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users
            .Where(user => user.Id == userId)
            .Select(user => new UserAccessInfo(user.Type, user.SchoolId))
            .FirstOrDefaultAsync(cancellationToken);

    private sealed record UserAccessInfo(UserType Type, Guid? SchoolId);
}
