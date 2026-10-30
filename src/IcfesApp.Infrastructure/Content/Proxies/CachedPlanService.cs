using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Plans.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace IcfesApp.Infrastructure.Content.Proxies;

/// <summary>
/// Caching proxy (Proxy pattern) in front of the real <see cref="PlanService"/>.
///
/// The plans catalog feeds the public pricing page and almost never changes. There is no API endpoint
/// that edits plans (they are managed directly in the database), so entries simply expire after
/// <see cref="CacheDuration"/>: a plan changed in the database shows up at most that long afterwards.
///
/// It implements the same <see cref="IPlanService"/> interface: controllers do not know it exists.
/// </summary>
public class CachedPlanService(IPlanService realPlanService, IMemoryCache memoryCache) : IPlanService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<IReadOnlyList<PlanDto>> GetPlansAsync(
        string? target = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        // One cache entry per filter combination, e.g. "plans:particular:active".
        var normalizedTarget = string.IsNullOrWhiteSpace(target) ? "all" : target.Trim().ToLowerInvariant();
        var plansCacheKey = $"plans:{normalizedTarget}:{(includeInactive ? "all" : "active")}";

        var cachedPlans = await memoryCache.GetOrCreateAsync(plansCacheKey, cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return realPlanService.GetPlansAsync(target, includeInactive, cancellationToken);
        });

        return cachedPlans ?? [];
    }

    public async Task<PlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allPlans = await GetPlansAsync(target: null, includeInactive: true, cancellationToken);
        return allPlans.FirstOrDefault(plan => plan.Id == id);
    }
}
