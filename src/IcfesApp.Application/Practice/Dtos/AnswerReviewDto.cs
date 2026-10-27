namespace IcfesApp.Application.Practice.Dtos;

public class AnswerReviewDto
{
    public Guid QuestionId { get; set; }
    public required string QuestionText { get; set; }
    public string? QuestionImageUrl { get; set; } // <--- AGREGADO
    public Guid? SelectedOptionId { get; set; }
    public Guid CorrectOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public required string Justification { get; set; }
    public string? JustificationImageUrl { get; set; } // <--- AGREGADO
}