using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Subjects;
using IcfesApp.Application.Subjects.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class SubjectService(IApplicationDbContext dbContext) : ISubjectService
{
    public async Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subjects = await dbContext.Subjects
            .AsNoTracking()
            .OrderBy(subject => subject.Name)
            .ToListAsync(cancellationToken);

        return subjects.Select(subject => subject.ToDto()).ToList();
    }

    public async Task<SubjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subject = await dbContext.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(subject => subject.Id == id, cancellationToken);

        return subject?.ToDto();
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var subject = new Subject { Name = request.Name, Description = request.Description };

        dbContext.Subjects.Add(subject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return subject.ToDto();
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var subject = await dbContext.Subjects.FirstOrDefaultAsync(subject => subject.Id == id, cancellationToken);
        if (subject is null)
        {
            return Result.NotFound();
        }

        subject.Name = request.Name;
        subject.Description = request.Description;
        subject.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subject = await dbContext.Subjects.FirstOrDefaultAsync(subject => subject.Id == id, cancellationToken);
        if (subject is null)
        {
            return Result.NotFound();
        }

        dbContext.Subjects.Remove(subject);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failed(["No se puede eliminar: la materia tiene preguntas asociadas."]);
        }

        return Result.Success();
    }
}
