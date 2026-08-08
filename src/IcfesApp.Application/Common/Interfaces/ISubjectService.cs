using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Subjects.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
