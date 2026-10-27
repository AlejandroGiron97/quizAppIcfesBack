using IcfesApp.Application.Common.Interfaces;
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
        var query = dbContext.Plans.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(target) &&
            Enum.TryParse<PlanTarget>(target, ignoreCase: true, out var parsedTarget))
        {
            query = query.Where(p => p.Target == parsedTarget);
        }

        return await query
            .OrderBy(p => p.Target)
            .ThenBy(p => p.DisplayOrder)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Target = p.Target.ToString(),
                DurationDays = p.DurationDays,
                Price = p.Price,
                Currency = p.Currency,
                IsActive = p.IsActive,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Plans
            .Where(p => p.Id == id)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Target = p.Target.ToString(),
                DurationDays = p.DurationDays,
                Price = p.Price,
                Currency = p.Currency,
                IsActive = p.IsActive,
                DisplayOrder = p.DisplayOrder
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}