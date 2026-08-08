using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class QuestionService(IApplicationDbContext dbContext) : IQuestionService
{
    public async Task<IReadOnlyList<QuestionDto>> GetAllAsync(Guid? subjectId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Questions
            .Include(q => q.Subject)
            .Include(q => q.Options)
            .AsQueryable();

        if (subjectId is not null)
        {
            query = query.Where(q => q.SubjectId == subjectId);
        }

        var questions = await query
            .OrderByDescending(q => q.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return questions.Select(ToDto).ToList();
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions
            .Include(q => q.Subject)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        return question is null ? null : ToDto(question);
    }

    public async Task<Result<QuestionDto>> CreateAsync(CreateQuestionRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateOptions(request.Options);
        if (validationErrors.Count > 0)
        {
            return Result<QuestionDto>.Failed(validationErrors);
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result<QuestionDto>.Failed(["La materia indicada no existe."]);
        }

        var question = new Question
        {
            Text = request.Text,
            Justification = request.Justification,
            Difficulty = request.Difficulty,
            SubjectId = request.SubjectId,
            CreatedByUserId = createdByUserId,
            Options = request.Options
                .Select(o => new QuestionOption { Text = o.Text, IsCorrect = o.IsCorrect })
                .ToList()
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(cancellationToken);

        question.Subject = subject;
        return Result<QuestionDto>.Success(ToDto(question));
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateOptions(request.Options);
        if (validationErrors.Count > 0)
        {
            return Result.Failed(validationErrors);
        }

        var question = await dbContext.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question is null)
        {
            return Result.NotFound();
        }

        var subjectExists = await dbContext.Subjects.AnyAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Result.Failed(["La materia indicada no existe."]);
        }

        question.Text = request.Text;
        question.Justification = request.Justification;
        question.Difficulty = request.Difficulty;
        question.SubjectId = request.SubjectId;
        question.UpdatedAtUtc = DateTime.UtcNow;

        // Reemplazo explícito de las opciones: al reasignar la navegación de un padre ya
        // trackeado, EF Core hace fixup automático y como las nuevas QuestionOption ya
        // tienen un Guid no vacío (por el inicializador de BaseEntity.Id), las marca como
        // Modified en vez de Added. Por eso se agregan explícitamente al DbSet con AddRange.
        var newOptions = request.Options
            .Select(option => new QuestionOption { Text = option.Text, IsCorrect = option.IsCorrect, QuestionId = question.Id })
            .ToList();

        dbContext.QuestionOptions.RemoveRange(question.Options);
        dbContext.QuestionOptions.AddRange(newOptions);
        question.Options = newOptions;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (question is null)
        {
            return Result.NotFound();
        }

        dbContext.Questions.Remove(question);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failed(["No se puede eliminar: la pregunta tiene respuestas de estudiantes asociadas."]);
        }

        return Result.Success();
    }

    private static List<string> ValidateOptions(List<CreateQuestionOptionRequest> options)
    {
        var errors = new List<string>();

        if (options.Count < 2)
        {
            errors.Add("La pregunta debe tener al menos 2 opciones.");
        }

        if (options.Count(o => o.IsCorrect) != 1)
        {
            errors.Add("La pregunta debe tener exactamente una opción correcta.");
        }

        return errors;
    }

    private static QuestionDto ToDto(Question question) => new()
    {
        Id = question.Id,
        Text = question.Text,
        Justification = question.Justification,
        Difficulty = question.Difficulty,
        SubjectId = question.SubjectId,
        SubjectName = question.Subject?.Name ?? string.Empty,
        CreatedByUserId = question.CreatedByUserId,
        Options = question.Options
            .Select(o => new QuestionOptionDto { Id = o.Id, Text = o.Text, IsCorrect = o.IsCorrect })
            .ToList()
    };
}
