using System.ComponentModel.DataAnnotations;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionRequest
{
    [Required]
    public required string Text { get; set; }

    /// <summary>Imagen opcional del enunciado (mapas, gráficas, tablas...).</summary>
    public string? ImageUrl { get; set; }

    [Required]
    public required string Justification { get; set; }

    /// <summary>Imagen opcional de la justificación (fórmulas, procedimientos...).</summary>
    public string? JustificationImageUrl { get; set; }

    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    [Required]
    public Guid SubjectId { get; set; }

    [Required, MinLength(2)]
    public required List<CreateQuestionOptionRequest> Options { get; set; }
}
