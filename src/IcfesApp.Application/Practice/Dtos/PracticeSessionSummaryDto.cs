namespace IcfesApp.Application.Practice.Dtos;

/// <summary>
/// One row of the practice history. Only counts, no questions: the history screen shows
/// subject, date, status and progress, so downloading every question would be wasted data.
/// </summary>
public class PracticeSessionSummaryDto
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public required string Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public int CorrectAnswers { get; set; }
}
