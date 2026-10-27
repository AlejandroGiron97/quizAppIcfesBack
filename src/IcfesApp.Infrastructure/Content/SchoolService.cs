using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Schools.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class SchoolService(IApplicationDbContext dbContext) : ISchoolService
{
    public async Task<IReadOnlyList<SchoolDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Schools
            .OrderBy(s => s.Name)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Domain = s.Domain,
                City = s.City,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SchoolDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Schools
            .Where(s => s.Id == id)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Domain = s.Domain,
                City = s.City,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SchoolDto> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var school = new School
        {
            Name = request.Name,
            Domain = request.Domain,
            City = request.City,
            IsActive = true
        };

        dbContext.Schools.Add(school);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Domain = school.Domain,
            City = school.City,
            IsActive = school.IsActive
        };
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var school = await dbContext.Schools.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (school is null)
        {
            return Result.NotFound();
        }

        school.Name = request.Name;
        school.Domain = request.Domain;
        school.City = request.City;
        school.IsActive = request.IsActive;
        school.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var school = await dbContext.Schools.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (school is null)
        {
            return Result.NotFound();
        }

        dbContext.Schools.Remove(school);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}