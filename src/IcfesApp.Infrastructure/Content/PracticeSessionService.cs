using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice.Dtos;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class PracticeSessionService(IApplicationDbContext dbContext) : IPracticeSessionService
{
    public async Task<Result<PracticeSessionDto>> StartAsync(StartPracticeSessionRequest request, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        if (request.QuestionCount is <= 0)
        {
            return Result<PracticeSessionDto>.Failed(["QuestionCount debe ser mayor a 0."]);
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result<PracticeSessionDto>.Failed(["La materia indicada no existe."]);
        }

        var questionsQuery = dbContext.Questions
            .Include(q => q.Options)
            .Where(q => q.SubjectId == request.SubjectId)
            .OrderBy(_ => Guid.NewGuid());

        var questions = request.QuestionCount is int count
            ? await questionsQuery.Take(count).ToListAsync(cancellationToken)
            : await questionsQuery.ToListAsync(cancellationToken);

        if (questions.Count == 0)
        {
            return Result<PracticeSessionDto>.Failed(["La materia no tiene preguntas disponibles."]);
        }

        var session = new PracticeSession
        {
            StudentUserId = studentUserId,
            SubjectId = request.SubjectId,
            Status = PracticeSessionStatus.InProgress,
            Answers = questions.Select(q => new StudentAnswer { QuestionId = q.Id, Question = q, IsCorrect = false }).ToList()
        };

        dbContext.PracticeSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PracticeSessionDto>.Success(ToDto(session, subject));
    }

    public async Task<PracticeSessionDto?> GetByIdAsync(Guid id, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(id, studentUserId, cancellationToken);
        return session is null ? null : ToDto(session, session.Subject);
    }

    public async Task<IReadOnlyList<PracticeSessionDto>> GetMineAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var sessions = await dbContext.PracticeSessions
            .Include(s => s.Subject)
            .Include(s => s.Answers).ThenInclude(a => a.Question).ThenInclude(q => q.Options)
            .Where(s => s.StudentUserId == studentUserId)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => ToDto(s, s.Subject))
            .ToList();
    }

    public async Task<Result<AnswerResultDto>> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, studentUserId, cancellationToken);
        if (session is null)
        {
            return Result<AnswerResultDto>.Failed(["Sesión no encontrada."]);
        }

        if (session.Status != PracticeSessionStatus.InProgress)
        {
            return Result<AnswerResultDto>.Failed(["La sesión ya fue finalizada."]);
        }

        var answer = session.Answers.FirstOrDefault(a => a.QuestionId == request.QuestionId);
        if (answer is null)
        {
            return Result<AnswerResultDto>.Failed(["La pregunta no pertenece a esta sesión."]);
        }

        var selectedOption = answer.Question.Options.FirstOrDefault(o => o.Id == request.SelectedOptionId);
        if (selectedOption is null)
        {
            return Result<AnswerResultDto>.Failed(["La opción indicada no pertenece a esta pregunta."]);
        }

        var correctOption = answer.Question.Options.First(o => o.IsCorrect);

        answer.SelectedOptionId = request.SelectedOptionId;
        answer.IsCorrect = selectedOption.IsCorrect;
        answer.AnsweredAtUtc = DateTime.UtcNow;
        answer.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AnswerResultDto>.Success(new AnswerResultDto
        {
            QuestionId = answer.QuestionId,
            IsCorrect = answer.IsCorrect,
            CorrectOptionId = correctOption.Id,
            Justification = answer.Question.Justification
        });
    }

    public async Task<Result<PracticeSessionResultDto>> FinishAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, studentUserId, cancellationToken);
        if (session is null)
        {
            return Result<PracticeSessionResultDto>.Failed(["Sesión no encontrada."]);
        }

        if (session.Status == PracticeSessionStatus.InProgress)
        {
            session.Status = PracticeSessionStatus.Completed;
            session.FinishedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var total = session.Answers.Count;
        var correct = session.Answers.Count(a => a.IsCorrect);

        return Result<PracticeSessionResultDto>.Success(new PracticeSessionResultDto
        {
            Id = session.Id,
            TotalQuestions = total,
            CorrectAnswers = correct,
            ScorePercentage = total == 0 ? 0 : Math.Round(correct * 100.0 / total, 2),
            Answers = session.Answers.Select(a => new AnswerReviewDto
            {
                QuestionId = a.QuestionId,
                QuestionText = a.Question.Text,
                SelectedOptionId = a.SelectedOptionId,
                CorrectOptionId = a.Question.Options.First(o => o.IsCorrect).Id,
                IsCorrect = a.IsCorrect,
                Justification = a.Question.Justification
            }).ToList()
        });
    }

    private async Task<PracticeSession?> LoadOwnedSessionAsync(Guid id, Guid studentUserId, CancellationToken cancellationToken)
    {
        var session = await dbContext.PracticeSessions
            .Include(s => s.Subject)
            .Include(s => s.Answers).ThenInclude(a => a.Question).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return session is null || session.StudentUserId != studentUserId ? null : session;
    }

    private static PracticeSessionDto ToDto(PracticeSession session, Subject subject) => new()
    {
        Id = session.Id,
        SubjectId = session.SubjectId,
        SubjectName = subject.Name,
        Status = session.Status.ToString(),
        StartedAtUtc = session.StartedAtUtc,
        FinishedAtUtc = session.FinishedAtUtc,
        Questions = session.Answers.Select(a => new PracticeQuestionDto
        {
            QuestionId = a.QuestionId,
            Text = a.Question.Text,
            Options = a.Question.Options
                .Select(o => new PracticeQuestionOptionDto { Id = o.Id, Text = o.Text })
                .ToList(),
            Answered = a.SelectedOptionId is not null,
            SelectedOptionId = a.SelectedOptionId
        }).ToList()
    };
}
