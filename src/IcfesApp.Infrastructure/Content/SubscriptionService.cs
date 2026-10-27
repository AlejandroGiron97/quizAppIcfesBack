using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Subscriptions.Dtos;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class SubscriptionService(ApplicationDbContext dbContext) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Type, u.SchoolId })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (user.Type == UserType.Institutional && user.SchoolId.HasValue)
        {
            return await GetForInstitutionAsync(user.SchoolId.Value, cancellationToken);
        }

        var subscription = await dbContext.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription is null ? null : MapToDto(subscription);
    }

    public async Task<SubscriptionDto?> GetForInstitutionAsync(
        Guid institutionId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await dbContext.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Institution)
            .Where(s => s.InstitutionId == institutionId)
            .OrderByDescending(s => s.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription is null ? null : MapToDto(subscription);
    }

    public async Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await dbContext.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Institution)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task<bool> HasActiveAccessAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Type, u.SchoolId })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        if (user.Type == UserType.Particular)
        {
            return await dbContext.Subscriptions.AnyAsync(s =>
                s.UserId == userId &&
                s.Status == SubscriptionStatus.Active &&
                s.ExpiresAtUtc > now,
                cancellationToken);
        }

        if (user.Type == UserType.Institutional && user.SchoolId.HasValue)
        {
            return await dbContext.Subscriptions.AnyAsync(s =>
                s.InstitutionId == user.SchoolId.Value &&
                s.Status == SubscriptionStatus.Active &&
                s.ExpiresAtUtc > now,
                cancellationToken);
        }

        return false;
    }

    private static SubscriptionDto MapToDto(Subscription s)
    {
        var now = DateTime.UtcNow;
        var daysRemaining = (int)Math.Ceiling((s.ExpiresAtUtc - now).TotalDays);

        return new SubscriptionDto
        {
            Id = s.Id,
            OwnerType = s.UserId.HasValue ? "Particular" : "Institutional",
            UserId = s.UserId,
            InstitutionId = s.InstitutionId,
            InstitutionName = s.Institution?.Name,
            PlanId = s.PlanId,
            PlanCode = s.Plan?.Code ?? string.Empty,
            PlanName = s.Plan?.Name ?? string.Empty,
            Status = s.Status.ToString(),
            StartedAtUtc = s.StartedAtUtc,
            ExpiresAtUtc = s.ExpiresAtUtc,
            CancelledAtUtc = s.CancelledAtUtc,
            AutoRenew = s.AutoRenew,
            IsCurrentlyActive = s.Status == SubscriptionStatus.Active && s.ExpiresAtUtc > now,
            DaysRemaining = daysRemaining
        };
    }
}