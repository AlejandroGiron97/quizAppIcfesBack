using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
