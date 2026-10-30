using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Questions.Dtos;

/// <summary>
/// Full question, including the correct option. Only teachers and administrators see it;
/// students see questions through the practice session (PracticeQuestionDto).
/// </summary>
public class QuestionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }
    public required string Justification { get; set; }
    public string? JustificationImageUrl { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public Guid SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = [];
}
