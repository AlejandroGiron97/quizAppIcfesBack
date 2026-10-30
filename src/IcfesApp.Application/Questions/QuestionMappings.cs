using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Questions;

/// <summary>Converts question entities to DTOs. The only place where a QuestionDto is built.</summary>
public static class QuestionMappings
{
    /// <remarks>Requires <see cref="Question.Subject"/> and <see cref="Question.Options"/> to be loaded.</remarks>
    public static QuestionDto ToDto(this Question question) => new()
    {
        Id = question.Id,
        Text = question.Text,
        ImageUrl = question.ImageUrl,
        Justification = question.Justification,
        JustificationImageUrl = question.JustificationImageUrl,
        Difficulty = question.Difficulty,
        SubjectId = question.SubjectId,
        SubjectName = question.Subject?.Name ?? string.Empty,
        CreatedByUserId = question.CreatedByUserId,
        Options = question.Options.Select(option => option.ToDto()).ToList()
    };

    public static QuestionOptionDto ToDto(this QuestionOption option) => new()
    {
        Id = option.Id,
        Text = option.Text,
        ImageUrl = option.ImageUrl,
        IsCorrect = option.IsCorrect
    };
}
