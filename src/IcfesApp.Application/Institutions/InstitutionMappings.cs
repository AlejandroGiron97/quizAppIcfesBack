using IcfesApp.Application.Institutions.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Institutions;

public static class InstitutionMappings
{
    public static InstitutionDto ToDto(this Institution institution) => new()
    {
        Id = institution.Id,
        Name = institution.Name,
        Domain = institution.Domain,
        City = institution.City,
        IsActive = institution.IsActive
    };
}
