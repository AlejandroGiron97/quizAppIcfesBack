using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

public class Question : BaseEntity
{
    public required string Text { get; set; }

    // URL de la imagen principal de la pregunta (mapas, gráficas, etc.)
    public string? ImageUrl { get; set; }

    public required string Justification { get; set; }

    // URL de la imagen de la justificación (por si tiene fórmulas complejas)
    public string? JustificationImageUrl { get; set; }

    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    // Relación con las opciones (que también tienen ImageUrl)
    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];
}