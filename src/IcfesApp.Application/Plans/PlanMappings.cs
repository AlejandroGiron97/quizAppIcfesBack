using IcfesApp.Application.Plans.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Plans;

public static class PlanMappings
{
    public static PlanDto ToDto(this Plan plan) => new()
    {
        Id = plan.Id,
        Code = plan.Code,
        Name = plan.Name,
        Description = plan.Description,
        Target = plan.Target.ToString(),
        DurationDays = plan.DurationDays,
        Price = plan.Price,
        Currency = plan.Currency,
        IsActive = plan.IsActive,
        DisplayOrder = plan.DisplayOrder
    };
}
