using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Institutions.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IInstitutionService
{
    Task<IReadOnlyList<InstitutionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<InstitutionDto> CreateAsync(CreateInstitutionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the updated institution so the client can refresh its list without reloading it.</summary>
    Task<Result<InstitutionDto>> UpdateAsync(Guid id, UpdateInstitutionRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
