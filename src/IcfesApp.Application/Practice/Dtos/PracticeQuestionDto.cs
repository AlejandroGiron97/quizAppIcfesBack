namespace IcfesApp.Application.Practice.Dtos;

public class PracticeQuestionDto
{
    public Guid QuestionId { get; set; }
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }
    public List<PracticeQuestionOptionDto> Options { get; set; } = [];
    public bool Answered { get; set; }
    public Guid? SelectedOptionId { get; set; }
}