using IcfesApp.Application.Schools.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Schools;

public static class SchoolMappings
{
    public static SchoolDto ToDto(this School school) => new()
    {
        Id = school.Id,
        Name = school.Name,
        Domain = school.Domain,
        City = school.City,
        IsActive = school.IsActive
    };
}
