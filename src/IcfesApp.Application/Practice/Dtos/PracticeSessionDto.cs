namespace IcfesApp.Application.Practice.Dtos;

public class PracticeSessionDto
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public required string Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public List<PracticeQuestionDto> Questions { get; set; } = [];
}
