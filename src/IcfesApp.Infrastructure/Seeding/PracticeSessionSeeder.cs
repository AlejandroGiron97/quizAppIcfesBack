using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Practice history for the demo students, so reports and dashboards have data:
/// completed sessions with different scores, one in progress and one abandoned.
/// </summary>
public class PracticeSessionSeeder(ApplicationDbContext dbContext) : DemoSeeder(dbContext)
{
    private const int QuestionsPerSession = 5;

    protected override Task<bool> IsAlreadySeededAsync() => DbContext.PracticeSessions.AnyAsync();

    protected override async Task InsertDataAsync()
    {
        var student1Id = await FindUserIdAsync(DemoData.Student1Email);
        var student2Id = await FindUserIdAsync(DemoData.Student2Email);
        var student3Id = await FindUserIdAsync(DemoData.Student3Email);
        var student4Id = await FindUserIdAsync(DemoData.Student4Email);
        if (student1Id is null || student2Id is null || student3Id is null || student4Id is null) return;

        var mathematicsId = await GetSubjectIdAsync(DemoData.Mathematics);
        var criticalReadingId = await GetSubjectIdAsync(DemoData.CriticalReading);
        var naturalSciencesId = await GetSubjectIdAsync(DemoData.NaturalSciences);
        var englishId = await GetSubjectIdAsync(DemoData.English);

        await CreateCompletedSessionAsync(student1Id.Value, mathematicsId, DateTime.UtcNow.AddDays(-5), correctAnswerCount: 4);
        await CreateCompletedSessionAsync(student1Id.Value, criticalReadingId, DateTime.UtcNow.AddDays(-3), correctAnswerCount: 3);
        await CreateCompletedSessionAsync(student2Id.Value, mathematicsId, DateTime.UtcNow.AddDays(-2), correctAnswerCount: 1);
        await CreateInProgressSessionAsync(student3Id.Value, naturalSciencesId, answeredQuestionCount: 2);
        await CreateAbandonedSessionAsync(student4Id.Value, englishId);
    }

    /// <summary>The first <paramref name="correctAnswerCount"/> questions are answered correctly, the rest incorrectly.</summary>
    private async Task CreateCompletedSessionAsync(Guid studentId, Guid subjectId, DateTime startedAtUtc, int correctAnswerCount)
    {
        var questions = await LoadFirstQuestionsAsync(subjectId);
        if (questions.Count == 0) return;

        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.Completed,
            StartedAtUtc = startedAtUtc,
            FinishedAtUtc = startedAtUtc.AddMinutes(45)
        };

        for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
        {
            var question = questions[questionIndex];
            var answeredCorrectly = questionIndex < correctAnswerCount;
            var wrongOption = question.Options.FirstOrDefault(option => !option.IsCorrect) ?? question.CorrectOption;

            session.Answers.Add(new StudentAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = (answeredCorrectly ? question.CorrectOption : wrongOption).Id,
                IsCorrect = answeredCorrectly,
                AnsweredAtUtc = startedAtUtc.AddMinutes(questionIndex * 5 + 2)
            });
        }

        DbContext.PracticeSessions.Add(session);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>The first <paramref name="answeredQuestionCount"/> questions are answered (correctly); the rest are still open.</summary>
    private async Task CreateInProgressSessionAsync(Guid studentId, Guid subjectId, int answeredQuestionCount)
    {
        var questions = await LoadFirstQuestionsAsync(subjectId);
        if (questions.Count == 0) return;

        var startedAtUtc = DateTime.UtcNow.AddMinutes(-20);
        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.InProgress,
            StartedAtUtc = startedAtUtc
        };

        for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
        {
            var question = questions[questionIndex];
            var isAnswered = questionIndex < answeredQuestionCount;

            session.Answers.Add(new StudentAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = isAnswered ? question.CorrectOption.Id : null,
                IsCorrect = isAnswered,
                AnsweredAtUtc = isAnswered ? startedAtUtc.AddMinutes(questionIndex * 3 + 1) : null
            });
        }

        DbContext.PracticeSessions.Add(session);
        await DbContext.SaveChangesAsync();
    }

    private async Task CreateAbandonedSessionAsync(Guid studentId, Guid subjectId)
    {
        var startedAtUtc = DateTime.UtcNow.AddDays(-1);

        DbContext.PracticeSessions.Add(new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.Abandoned,
            StartedAtUtc = startedAtUtc,
            FinishedAtUtc = startedAtUtc.AddMinutes(15)
        });

        await DbContext.SaveChangesAsync();
    }

    /// <summary>Always the same questions (ordered by text), so the demo data is predictable.</summary>
    private Task<List<Question>> LoadFirstQuestionsAsync(Guid subjectId) =>
        DbContext.Questions
            .Include(question => question.Options)
            .Where(question => question.SubjectId == subjectId)
            .OrderBy(question => question.Text)
            .Take(QuestionsPerSession)
            .ToListAsync();
}
