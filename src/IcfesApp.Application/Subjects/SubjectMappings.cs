using IcfesApp.Application.Subjects.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Subjects;

public static class SubjectMappings
{
    public static SubjectDto ToDto(this Subject subject) => new()
    {
        Id = subject.Id,
        Name = subject.Name,
        Description = subject.Description
    };
}
