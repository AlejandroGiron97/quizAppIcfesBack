using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Builders;

/// <summary>
/// Construye preguntas válidas paso a paso (patrón Builder).
///
/// Es el ÚNICO lugar donde se arma una <see cref="Question"/> con sus opciones, así las reglas
/// del negocio viven en un solo sitio y las usan por igual el servicio (crear/editar) y el seeder:
///   - Debe tener enunciado y justificación.
///   - Debe tener al menos <see cref="MinimumOptionCount"/> opciones.
///   - Debe tener exactamente una opción correcta.
///
/// Uso:
/// <code>
/// var questionBuilder = QuestionBuilder.ForSubject(subjectId)
///     .WithStatement("¿Cuánto es 2 + 2?", statementImageUrl: null)
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

    /// <summary>Punto de entrada: toda pregunta pertenece a una materia.</summary>
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

    /// <summary>Revisa las reglas del negocio. Lista vacía = la pregunta se puede construir.</summary>
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

        if (_optionDrafts.Any(optionDraft => string.IsNullOrWhiteSpace(optionDraft.Text)))
        {
            validationErrors.Add("Todas las opciones deben tener texto.");
        }

        return validationErrors;
    }

    /// <summary>
    /// Crea la pregunta. Lanza <see cref="InvalidOperationException"/> si no cumple las reglas:
    /// llama antes a <see cref="Validate"/> para obtener los errores sin excepción.
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
    /// Crea solo las opciones, ya validadas, para una pregunta existente.
    /// Se usa al editar, donde la pregunta ya está en la base de datos.
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
                $"No se puede construir la pregunta: {string.Join(" ", validationErrors)}");
        }
    }

    private static string? NormalizeImageUrl(string? imageUrl) =>
        string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();

    private sealed record QuestionOptionDraft(string Text, bool IsCorrect, string? ImageUrl);
}
