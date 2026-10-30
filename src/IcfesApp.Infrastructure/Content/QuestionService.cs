using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Questions;
using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Builders;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class QuestionService(IApplicationDbContext dbContext) : IQuestionService
{
    private const string SubjectNotFoundMessage = "La materia indicada no existe.";

    public async Task<IReadOnlyList<QuestionDto>> GetAllAsync(Guid? subjectId, CancellationToken cancellationToken = default)
    {
        var questionsQuery = dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Subject)
            .Include(question => question.Options)
            .AsQueryable();

        if (subjectId is not null)
        {
            questionsQuery = questionsQuery.Where(question => question.SubjectId == subjectId);
        }

        var questions = await questionsQuery
            .OrderByDescending(question => question.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return questions.Select(question => question.ToDto()).ToList();
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Subject)
            .Include(question => question.Options)
            .FirstOrDefaultAsync(question => question.Id == id, cancellationToken);

        return question?.ToDto();
    }

    public async Task<Result<QuestionDto>> CreateAsync(CreateQuestionRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var questionBuilder = CreateBuilderFrom(request).CreatedBy(createdByUserId);

        var validationErrors = questionBuilder.Validate();
        if (validationErrors.Count > 0)
        {
            return Result<QuestionDto>.Failed(validationErrors);
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(subject => subject.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result<QuestionDto>.Failed([SubjectNotFoundMessage]);
        }

        var question = questionBuilder.Build();
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(cancellationToken);

        question.Subject = subject;
        return Result<QuestionDto>.Success(question.ToDto());
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var questionBuilder = CreateBuilderFrom(request);

        var validationErrors = questionBuilder.Validate();
        if (validationErrors.Count > 0)
        {
            return Result.Failed(validationErrors);
        }

        var existingQuestion = await dbContext.Questions
            .Include(question => question.Options)
            .FirstOrDefaultAsync(question => question.Id == id, cancellationToken);

        if (existingQuestion is null)
        {
            return Result.NotFound();
        }

        var subjectExists = await dbContext.Subjects.AnyAsync(subject => subject.Id == request.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Result.Failed([SubjectNotFoundMessage]);
        }

        existingQuestion.Text = request.Text.Trim();
        existingQuestion.ImageUrl = request.ImageUrl;
        existingQuestion.Justification = request.Justification.Trim();
        existingQuestion.JustificationImageUrl = request.JustificationImageUrl;
        existingQuestion.Difficulty = request.Difficulty;
        existingQuestion.SubjectId = request.SubjectId;
        existingQuestion.UpdatedAtUtc = DateTime.UtcNow;

        ReplaceOptions(existingQuestion, questionBuilder.BuildOptionsFor(existingQuestion.Id));

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions.FirstOrDefaultAsync(question => question.Id == id, cancellationToken);
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

    /// <summary>Maps the HTTP request to the domain builder, which is the one that validates the rules.</summary>
    private static QuestionBuilder CreateBuilderFrom(CreateQuestionRequest request)
    {
        var questionBuilder = QuestionBuilder.ForSubject(request.SubjectId)
            .WithStatement(request.Text, request.ImageUrl)
            .WithJustification(request.Justification, request.JustificationImageUrl)
            .WithDifficulty(request.Difficulty);

        foreach (var optionRequest in request.Options)
        {
            questionBuilder.AddOption(optionRequest.Text, optionRequest.IsCorrect, optionRequest.ImageUrl);
        }

        return questionBuilder;
    }

    /// <summary>
    /// Replaces the options of a question already tracked by EF Core.
    /// The new options are added explicitly with AddRange: since they already have a Guid
    /// (assigned by BaseEntity), EF would mark them as Modified instead of Added if only the
    /// navigation were reassigned.
    /// </summary>
    private void ReplaceOptions(Question existingQuestion, List<QuestionOption> newOptions)
    {
        dbContext.QuestionOptions.RemoveRange(existingQuestion.Options);
        dbContext.QuestionOptions.AddRange(newOptions);
        existingQuestion.Options = newOptions;
    }
}
