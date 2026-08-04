using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class StudentAnswer : BaseEntity
{
    public Guid PracticeSessionId { get; set; }
    public PracticeSession PracticeSession { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public Guid? SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }
    public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;
}
