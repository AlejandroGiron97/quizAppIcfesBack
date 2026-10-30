using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionOptionRequest
{
    [Required]
    public required string Text { get; set; }

    /// <summary>Optional image for the option (charts, diagrams...).</summary>
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }
}
