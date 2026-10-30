using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionOptionRequest
{
    /// <summary>
    /// May be empty when the option is only an image. The "text or image" rule is checked by QuestionBuilder.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public required string Text { get; set; }

    /// <summary>Optional image for the option (charts, diagrams...).</summary>
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }
}
