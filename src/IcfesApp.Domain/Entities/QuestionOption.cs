using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public required string Text { get; set; }

    // URL de la imagen de la opción (gráficas, diagramas, etc.)
    // Es nullable porque la mayoría de opciones solo tendrán texto.
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}