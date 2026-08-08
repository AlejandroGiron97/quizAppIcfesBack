using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Schools.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISchoolService
{
    Task<IReadOnlyList<SchoolDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SchoolDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SchoolDto> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
