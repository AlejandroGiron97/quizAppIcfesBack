using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Schools.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface ISchoolService
{
    Task<IReadOnlyList<SchoolDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SchoolDto> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the updated institution so the client can refresh its list without reloading it.</summary>
    Task<Result<SchoolDto>> UpdateAsync(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
