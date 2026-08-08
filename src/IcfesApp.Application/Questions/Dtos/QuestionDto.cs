using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Questions.Dtos;

public class QuestionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public required string Justification { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public Guid SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = [];
}
