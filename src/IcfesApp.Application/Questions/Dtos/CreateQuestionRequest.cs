using System.ComponentModel.DataAnnotations;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionRequest
{
    [Required]
    public required string Text { get; set; }

    /// <summary>Optional image for the statement (maps, charts, tables...).</summary>
    public string? ImageUrl { get; set; }

    [Required]
    public required string Justification { get; set; }

    /// <summary>Optional image for the justification (formulas, worked steps...).</summary>
    public string? JustificationImageUrl { get; set; }

    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    [Required]
    public Guid SubjectId { get; set; }

    [Required, MinLength(2)]
    public required List<CreateQuestionOptionRequest> Options { get; set; }
}
