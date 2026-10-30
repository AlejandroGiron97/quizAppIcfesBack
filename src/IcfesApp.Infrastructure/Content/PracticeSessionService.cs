using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice;
using IcfesApp.Application.Practice.Dtos;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class PracticeSessionService(IApplicationDbContext dbContext) : IPracticeSessionService
{
    private const string SessionNotFoundMessage = "Sesión no encontrada.";

    public async Task<Result<PracticeSessionDto>> StartAsync(StartPracticeSessionRequest request, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        if (request.QuestionCount is <= 0)
        {
            return Result<PracticeSessionDto>.Failed(["QuestionCount debe ser mayor a 0."]);
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(subject => subject.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result<PracticeSessionDto>.Failed(["La materia indicada no existe."]);
        }

        var selectedQuestions = await PickRandomQuestionsAsync(request.SubjectId, request.QuestionCount, cancellationToken);
        if (selectedQuestions.Count == 0)
        {
            return Result<PracticeSessionDto>.Failed(["La materia no tiene preguntas disponibles."]);
        }

        var session = new PracticeSession
        {
            StudentUserId = studentUserId,
            SubjectId = subject.Id,
            Subject = subject,
            Status = PracticeSessionStatus.InProgress,
            Answers = selectedQuestions
                .Select(question => new StudentAnswer { QuestionId = question.Id, Question = question })
                .ToList()
        };

        dbContext.PracticeSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PracticeSessionDto>.Success(session.ToDto());
    }

    public async Task<PracticeSessionDto?> GetByIdAsync(Guid id, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await FindStudentSessionAsync(id, studentUserId, cancellationToken);
        return session?.ToDto();
    }

    public async Task<IReadOnlyList<PracticeSessionDto>> GetMineAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var studentSessions = await SessionsWithQuestions()
            .AsNoTracking()
            .Where(session => session.StudentUserId == studentUserId)
            .OrderByDescending(session => session.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return studentSessions.Select(session => session.ToDto()).ToList();
    }

    public async Task<Result<AnswerResultDto>> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await FindStudentSessionAsync(sessionId, studentUserId, cancellationToken);
        if (session is null)
        {
            return Result<AnswerResultDto>.NotFound(SessionNotFoundMessage);
        }

        if (session.Status != PracticeSessionStatus.InProgress)
        {
            return Result<AnswerResultDto>.Failed(["La sesión ya fue finalizada."]);
        }

        var answer = session.Answers.FirstOrDefault(answer => answer.QuestionId == request.QuestionId);
        if (answer is null)
        {
            return Result<AnswerResultDto>.Failed(["La pregunta no pertenece a esta sesión."]);
        }

        var selectedOption = answer.Question.Options.FirstOrDefault(option => option.Id == request.SelectedOptionId);
        if (selectedOption is null)
        {
            return Result<AnswerResultDto>.Failed(["La opción indicada no pertenece a esta pregunta."]);
        }

        answer.RegisterSelectedOption(selectedOption);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AnswerResultDto>.Success(answer.ToAnswerResultDto());
    }

    public async Task<Result<PracticeSessionResultDto>> FinishAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var session = await FindStudentSessionAsync(sessionId, studentUserId, cancellationToken);
        if (session is null)
        {
            return Result<PracticeSessionResultDto>.NotFound(SessionNotFoundMessage);
        }

        if (session.Status == PracticeSessionStatus.InProgress)
        {
            session.Status = PracticeSessionStatus.Completed;
            session.FinishedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<PracticeSessionResultDto>.Success(session.ToResultDto());
    }

    /// <summary>
    /// Elige preguntas al azar de una materia. Primero trae solo los Ids (consulta liviana),
    /// los mezcla en memoria y después carga con sus opciones únicamente las elegidas.
    /// </summary>
    private async Task<List<Question>> PickRandomQuestionsAsync(Guid subjectId, int? requestedQuestionCount, CancellationToken cancellationToken)
    {
        var subjectQuestionIds = await dbContext.Questions
            .Where(question => question.SubjectId == subjectId)
            .Select(question => question.Id)
            .ToListAsync(cancellationToken);

        var selectedQuestionIds = subjectQuestionIds
            .OrderBy(_ => Random.Shared.Next())
            .Take(requestedQuestionCount ?? subjectQuestionIds.Count)
            .ToList();

        var selectedQuestions = await dbContext.Questions
            .Include(question => question.Options)
            .Where(question => selectedQuestionIds.Contains(question.Id))
            .ToListAsync(cancellationToken);

        // La consulta SQL no respeta el orden aleatorio: se reordena según la selección.
        return selectedQuestions
            .OrderBy(question => selectedQuestionIds.IndexOf(question.Id))
            .ToList();
    }

    /// <summary>Busca una sesión solo si pertenece al estudiante; si es de otro devuelve null.</summary>
    private Task<PracticeSession?> FindStudentSessionAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken) =>
        SessionsWithQuestions()
            .FirstOrDefaultAsync(
                session => session.Id == sessionId && session.StudentUserId == studentUserId,
                cancellationToken);

    private IQueryable<PracticeSession> SessionsWithQuestions() => dbContext.PracticeSessions
        .Include(session => session.Subject)
        .Include(session => session.Answers)
            .ThenInclude(answer => answer.Question)
            .ThenInclude(question => question.Options);
}
