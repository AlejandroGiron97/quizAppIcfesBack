using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

public class Question : BaseEntity
{
    /// <summary>Enunciado de la pregunta.</summary>
    public required string Text { get; set; }

    /// <summary>Imagen opcional del enunciado (mapas, gráficas, tablas...).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Explicación de la respuesta correcta que ve el estudiante después de responder.</summary>
    public required string Justification { get; set; }

    /// <summary>Imagen opcional de la justificación (fórmulas, procedimientos...).</summary>
    public string? JustificationImageUrl { get; set; }

    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    /// <summary>Opciones de respuesta. Siempre hay exactamente una correcta (lo garantiza QuestionBuilder).</summary>
    public ICollection<QuestionOption> Options { get; set; } = [];

    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];

    /// <summary>La opción correcta. Requiere que <see cref="Options"/> esté cargada. No se guarda en BD.</summary>
    public QuestionOption CorrectOption => Options.Single(option => option.IsCorrect);
}