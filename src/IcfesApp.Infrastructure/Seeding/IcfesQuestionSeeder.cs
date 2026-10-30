using System.Text.Json;
using IcfesApp.Domain.Builders;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Real ICFES questions read from preguntas_icfes.json (copied next to the API binaries).
/// Every question goes through <see cref="QuestionBuilder"/>, so the seed data follows the same rules as the API.
/// </summary>
public class IcfesQuestionSeeder(ApplicationDbContext dbContext, ILogger<IcfesQuestionSeeder> logger)
    : DemoSeeder(dbContext)
{
    private const string QuestionsSeedFileName = "preguntas_icfes.json";
    private const int MaxLoggedTextLength = 80;

    /// <summary>
    /// Questions are checked one by one (by subject + text), so new questions added to the JSON
    /// are inserted on the next startup without duplicating the existing ones.
    /// </summary>
    protected override Task<bool> IsAlreadySeededAsync() => Task.FromResult(false);

    protected override async Task InsertDataAsync()
    {
        var adminUserId = await FindUserIdAsync(RoleSeeder.DefaultAdminEmail);
        if (adminUserId is null) return;

        var questionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, QuestionsSeedFileName);
        if (!File.Exists(questionsFilePath)) return;

        var questionsJson = await File.ReadAllTextAsync(questionsFilePath);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var questionSeeds = JsonSerializer.Deserialize<List<QuestionSeed>>(questionsJson, jsonOptions);
        if (questionSeeds is null) return;

        var subjectIdsByName = await DbContext.Subjects.ToDictionaryAsync(subject => subject.Name, subject => subject.Id);

        // A single query to know which questions already exist (it used to be one query per question).
        // Both sides are trimmed: databases seeded by older versions stored the text untrimmed,
        // and comparing trimmed with untrimmed text would insert those questions again.
        var existingQuestionKeys = (await DbContext.Questions
                .Select(question => new { question.SubjectId, question.Text })
                .ToListAsync())
            .Select(existingQuestion => (existingQuestion.SubjectId, existingQuestion.Text.Trim()))
            .ToHashSet();

        foreach (var questionSeed in questionSeeds)
        {
            if (!subjectIdsByName.TryGetValue(questionSeed.Subject, out var subjectId)) continue;
            if (existingQuestionKeys.Contains((subjectId, questionSeed.Text.Trim()))) continue;

            var questionBuilder = CreateBuilderFrom(questionSeed, subjectId, adminUserId.Value);

            // A badly written question in the JSON is skipped instead of breaking startup,
            // but it is logged so it never disappears silently.
            var validationErrors = questionBuilder.Validate();
            if (validationErrors.Count > 0)
            {
                logger.LogWarning(
                    "Skipped seed question \"{QuestionText}\" ({Subject}): {ValidationErrors}",
                    Shorten(questionSeed.Text),
                    questionSeed.Subject,
                    string.Join(" ", validationErrors));
                continue;
            }

            DbContext.Questions.Add(questionBuilder.Build());
        }

        await DbContext.SaveChangesAsync();
    }

    private static QuestionBuilder CreateBuilderFrom(QuestionSeed questionSeed, Guid subjectId, Guid createdByUserId)
    {
        var difficulty = Enum.TryParse<QuestionDifficulty>(questionSeed.Difficulty, ignoreCase: true, out var parsedDifficulty)
            ? parsedDifficulty
            : QuestionDifficulty.Medium;

        var questionBuilder = QuestionBuilder.ForSubject(subjectId)
            .WithStatement(questionSeed.Text, questionSeed.ImageUrl)
            .WithJustification(questionSeed.Justification, questionSeed.JustificationImageUrl)
            .WithDifficulty(difficulty)
            .CreatedBy(createdByUserId);

        foreach (var optionSeed in questionSeed.Options)
        {
            questionBuilder.AddOption(optionSeed.Text, optionSeed.IsCorrect, optionSeed.ImageUrl);
        }

        return questionBuilder;
    }

    private static string Shorten(string text) =>
        text.Length > MaxLoggedTextLength ? text[..MaxLoggedTextLength] + "..." : text;

    /// <summary>Shape of each question in preguntas_icfes.json.</summary>
    private sealed class QuestionSeed
    {
        public string Subject { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string? JustificationImageUrl { get; set; }
        public string Difficulty { get; set; } = nameof(QuestionDifficulty.Medium);
        public List<OptionSeed> Options { get; set; } = [];
    }

    /// <summary>Shape of each option in preguntas_icfes.json. Text can be empty when the option is only an image.</summary>
    private sealed class OptionSeed
    {
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsCorrect { get; set; }
    }
}
