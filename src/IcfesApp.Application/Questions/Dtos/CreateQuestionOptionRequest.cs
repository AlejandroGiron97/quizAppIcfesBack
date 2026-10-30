using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionOptionRequest
{
    [Required]
    public required string Text { get; set; }

    /// <summary>Imagen opcional de la opción (gráficas, diagramas...).</summary>
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }
}
