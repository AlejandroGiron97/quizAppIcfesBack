using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public required string Text { get; set; }

    // URL of the option image (charts, diagrams, etc.)
    // Nullable because most options only have text.
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}