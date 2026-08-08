namespace IcfesApp.Application.Practice.Dtos;

public class AnswerResultDto
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public Guid CorrectOptionId { get; set; }
    public required string Justification { get; set; }
}
