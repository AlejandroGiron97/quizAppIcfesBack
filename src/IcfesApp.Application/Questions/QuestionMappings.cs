using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Questions;

/// <summary>Convierte entidades de preguntas a DTOs. Único lugar donde se arma un QuestionDto.</summary>
public static class QuestionMappings
{
    /// <remarks>Requiere que <see cref="Question.Subject"/> y <see cref="Question.Options"/> estén cargados.</remarks>
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
