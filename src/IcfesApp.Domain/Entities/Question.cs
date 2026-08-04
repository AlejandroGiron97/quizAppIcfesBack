using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

public class Question : BaseEntity
{
    public required string Text { get; set; }
    public required string Justification { get; set; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];
}
