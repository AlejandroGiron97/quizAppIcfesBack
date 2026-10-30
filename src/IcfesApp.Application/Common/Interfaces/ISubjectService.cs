using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Subjects.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the updated subject so the client can refresh its list without reloading it.</summary>
    Task<Result<SubjectDto>> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
