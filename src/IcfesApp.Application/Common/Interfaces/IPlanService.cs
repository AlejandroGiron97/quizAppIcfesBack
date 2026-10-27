using IcfesApp.Application.Plans.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IPlanService
{
    Task<IReadOnlyList<PlanDto>> GetPlansAsync(
        string? target = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<PlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}