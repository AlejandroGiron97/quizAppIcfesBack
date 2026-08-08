namespace IcfesApp.Application.Questions.Dtos;

public class QuestionOptionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}
