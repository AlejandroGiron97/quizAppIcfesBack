using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Builders;

/// <summary>
/// Builds valid questions step by step (Builder pattern).
///
/// It is the ONLY place where a <see cref="Question"/> is assembled with its options, so the business
/// rules live in a single spot and are shared by the service (create/update) and the seeder:
///   - It must have a statement and a justification.
///   - It must have at least <see cref="MinimumOptionCount"/> options.
///   - It must have exactly one correct option.
///   - Every option must have text, an image, or both (some ICFES options are only a chart).
///
/// Usage:
/// <code>
/// var questionBuilder = QuestionBuilder.ForSubject(subjectId)
///     .WithStatement("What is 2 + 2?", statementImageUrl: null)
///     .WithJustification("2 + 2 = 4")
///     .WithDifficulty(QuestionDifficulty.Easy)
///     .CreatedBy(teacherUserId)
///     .AddOption("4", isCorrect: true)
///     .AddOption("5", isCorrect: false);
///
/// var validationErrors = questionBuilder.Validate();
/// if (validationErrors.Count == 0)
/// {
///     var question = questionBuilder.Build();
/// }
/// </code>
/// </summary>
public sealed class QuestionBuilder
{
    public const int MinimumOptionCount = 2;

    private readonly Guid _subjectId;
    private readonly List<QuestionOptionDraft> _optionDrafts = [];

    private string _statementText = string.Empty;
    private string? _statementImageUrl;
    private string _justificationText = string.Empty;
    private string? _justificationImageUrl;
    private QuestionDifficulty _difficulty = QuestionDifficulty.Medium;
    private Guid _createdByUserId;

    private QuestionBuilder(Guid subjectId)
    {
        _subjectId = subjectId;
    }

    /// <summary>Entry point: every question belongs to a subject.</summary>
    public static QuestionBuilder ForSubject(Guid subjectId) => new(subjectId);

    public QuestionBuilder WithStatement(string statementText, string? statementImageUrl = null)
    {
        _statementText = statementText.Trim();
        _statementImageUrl = NormalizeImageUrl(statementImageUrl);
        return this;
    }

    public QuestionBuilder WithJustification(string justificationText, string? justificationImageUrl = null)
    {
        _justificationText = justificationText.Trim();
        _justificationImageUrl = NormalizeImageUrl(justificationImageUrl);
        return this;
    }

    public QuestionBuilder WithDifficulty(QuestionDifficulty difficulty)
    {
        _difficulty = difficulty;
        return this;
    }

    public QuestionBuilder CreatedBy(Guid createdByUserId)
    {
        _createdByUserId = createdByUserId;
        return this;
    }

    public QuestionBuilder AddOption(string optionText, bool isCorrect, string? optionImageUrl = null)
    {
        _optionDrafts.Add(new QuestionOptionDraft(optionText.Trim(), isCorrect, NormalizeImageUrl(optionImageUrl)));
        return this;
    }

    /// <summary>Checks the business rules. Empty list = the question can be built.</summary>
    public IReadOnlyList<string> Validate()
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(_statementText))
        {
            validationErrors.Add("La pregunta debe tener un enunciado.");
        }

        if (string.IsNullOrWhiteSpace(_justificationText))
        {
            validationErrors.Add("La pregunta debe tener una justificación.");
        }

        if (_optionDrafts.Count < MinimumOptionCount)
        {
            validationErrors.Add($"La pregunta debe tener al menos {MinimumOptionCount} opciones.");
        }

        if (_optionDrafts.Count(optionDraft => optionDraft.IsCorrect) != 1)
        {
            validationErrors.Add("La pregunta debe tener exactamente una opción correcta.");
        }

        // An option can be just an image (e.g. "which chart is correct?"), so text OR image is enough.
        if (_optionDrafts.Any(optionDraft => string.IsNullOrWhiteSpace(optionDraft.Text) && optionDraft.ImageUrl is null))
        {
            validationErrors.Add("Cada opción debe tener texto o imagen.");
        }

        return validationErrors;
    }

    /// <summary>
    /// Creates the question. Throws <see cref="InvalidOperationException"/> if the rules are not met:
    /// call <see cref="Validate"/> first to get the errors without an exception.
    /// </summary>
    public Question Build()
    {
        EnsureIsValid();

        var question = new Question
        {
            Text = _statementText,
            ImageUrl = _statementImageUrl,
            Justification = _justificationText,
            JustificationImageUrl = _justificationImageUrl,
            Difficulty = _difficulty,
            SubjectId = _subjectId,
            CreatedByUserId = _createdByUserId
        };

        question.Options = BuildOptionsFor(question.Id);
        return question;
    }

    /// <summary>
    /// Creates only the options, already validated, for an existing question.
    /// Used when updating, where the question is already in the database.
    /// </summary>
    public List<QuestionOption> BuildOptionsFor(Guid questionId)
    {
        EnsureIsValid();

        return _optionDrafts
            .Select(optionDraft => new QuestionOption
            {
                Text = optionDraft.Text,
                ImageUrl = optionDraft.ImageUrl,
                IsCorrect = optionDraft.IsCorrect,
                QuestionId = questionId
            })
            .ToList();
    }

    private void EnsureIsValid()
    {
        var validationErrors = Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"The question cannot be built: {string.Join(" ", validationErrors)}");
        }
    }

    private static string? NormalizeImageUrl(string? imageUrl) =>
        string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();

    private sealed record QuestionOptionDraft(string Text, bool IsCorrect, string? ImageUrl);
}
