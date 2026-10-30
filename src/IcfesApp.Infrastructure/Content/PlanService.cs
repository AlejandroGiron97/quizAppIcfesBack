using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Plans;
using IcfesApp.Application.Plans.Dtos;
using IcfesApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class PlanService(IApplicationDbContext dbContext) : IPlanService
{
    public async Task<IReadOnlyList<PlanDto>> GetPlansAsync(
        string? target = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var plansQuery = dbContext.Plans.AsNoTracking();

        if (!includeInactive)
        {
            plansQuery = plansQuery.Where(plan => plan.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(target) &&
            Enum.TryParse<PlanTarget>(target, ignoreCase: true, out var planTarget))
        {
            plansQuery = plansQuery.Where(plan => plan.Target == planTarget);
        }

        var plans = await plansQuery
            .OrderBy(plan => plan.Target)
            .ThenBy(plan => plan.DisplayOrder)
            .ToListAsync(cancellationToken);

        return plans.Select(plan => plan.ToDto()).ToList();
    }

    public async Task<PlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

        return plan?.ToDto();
    }
}
