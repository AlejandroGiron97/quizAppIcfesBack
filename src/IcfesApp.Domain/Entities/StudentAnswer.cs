using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// A question inside a practice session and the student's answer to it.
/// It is created unanswered when the session starts and completed with <see cref="RegisterSelectedOption"/>.
/// </summary>
public class StudentAnswer : BaseEntity
{
    public Guid PracticeSessionId { get; set; }
    public PracticeSession PracticeSession { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    /// <summary>Selected option. Null while the question has not been answered.</summary>
    public Guid? SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }

    /// <summary>When the student answered. Null while the question has not been answered.</summary>
    public DateTime? AnsweredAtUtc { get; set; }

    /// <summary>Not stored in the database.</summary>
    public bool IsAnswered => SelectedOptionId is not null;

    /// <summary>Stores the option chosen by the student and whether it was correct.</summary>
    public void RegisterSelectedOption(QuestionOption selectedOption)
    {
        var answeredAtUtc = DateTime.UtcNow;

        SelectedOptionId = selectedOption.Id;
        IsCorrect = selectedOption.IsCorrect;
        AnsweredAtUtc = answeredAtUtc;
        UpdatedAtUtc = answeredAtUtc;
    }
}
