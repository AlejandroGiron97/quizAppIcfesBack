using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Institutions;
using IcfesApp.Application.Institutions.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class InstitutionService(IApplicationDbContext dbContext) : IInstitutionService
{
    public async Task<IReadOnlyList<InstitutionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var institutions = await dbContext.Institutions
            .AsNoTracking()
            .OrderBy(institution => institution.Name)
            .ToListAsync(cancellationToken);

        return institutions.Select(institution => institution.ToDto()).ToList();
    }

    public async Task<InstitutionDto> CreateAsync(CreateInstitutionRequest request, CancellationToken cancellationToken = default)
    {
        var institution = new Institution
        {
            Name = request.Name,
            Domain = request.Domain,
            City = request.City,
            IsActive = true
        };

        dbContext.Institutions.Add(institution);
        await dbContext.SaveChangesAsync(cancellationToken);

        return institution.ToDto();
    }

    public async Task<Result<InstitutionDto>> UpdateAsync(Guid id, UpdateInstitutionRequest request, CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Institutions.FirstOrDefaultAsync(institution => institution.Id == id, cancellationToken);
        if (institution is null)
        {
            return Result<InstitutionDto>.NotFound();
        }

        institution.Name = request.Name;
        institution.Domain = request.Domain;
        institution.City = request.City;
        institution.IsActive = request.IsActive;
        institution.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<InstitutionDto>.Success(institution.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Institutions.FirstOrDefaultAsync(institution => institution.Id == id, cancellationToken);
        if (institution is null)
        {
            return Result.NotFound();
        }

        dbContext.Institutions.Remove(institution);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
