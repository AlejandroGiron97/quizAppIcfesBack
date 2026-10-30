using IcfesApp.Domain.Common;
using IcfesApp.Domain.Enums;

namespace IcfesApp.Domain.Entities;

public class Question : BaseEntity
{
    /// <summary>Question statement.</summary>
    public required string Text { get; set; }

    /// <summary>Optional image for the statement (maps, charts, tables...).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Explanation of the correct answer that the student sees after answering.</summary>
    public required string Justification { get; set; }

    /// <summary>Optional image for the justification (formulas, worked steps...).</summary>
    public string? JustificationImageUrl { get; set; }

    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    /// <summary>Answer options. There is always exactly one correct option (guaranteed by QuestionBuilder).</summary>
    public ICollection<QuestionOption> Options { get; set; } = [];

    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];

    /// <summary>The correct option. Requires <see cref="Options"/> to be loaded. Not stored in the database.</summary>
    public QuestionOption CorrectOption => Options.Single(option => option.IsCorrect);
}