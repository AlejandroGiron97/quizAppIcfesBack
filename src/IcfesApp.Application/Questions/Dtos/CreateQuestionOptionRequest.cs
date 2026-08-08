using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Questions.Dtos;

public class CreateQuestionOptionRequest
{
    [Required]
    public required string Text { get; set; }

    public bool IsCorrect { get; set; }
}
