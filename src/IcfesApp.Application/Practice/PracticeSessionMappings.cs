using IcfesApp.Application.Practice.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.Practice;

/// <summary>
/// Convierte sesiones de práctica a DTOs.
/// Mientras el estudiante responde (<see cref="ToDto"/>) nunca se expone cuál es la opción
/// correcta; solo se revela después de responder (<see cref="ToAnswerResultDto"/>) o al
/// finalizar (<see cref="ToResultDto"/>).
/// </summary>
public static class PracticeSessionMappings
{
    /// <remarks>Requiere Subject y Answers → Question → Options cargados.</remarks>
    public static PracticeSessionDto ToDto(this PracticeSession session) => new()
    {
        Id = session.Id,
        SubjectId = session.SubjectId,
        SubjectName = session.Subject.Name,
        Status = session.Status.ToString(),
        StartedAtUtc = session.StartedAtUtc,
        FinishedAtUtc = session.FinishedAtUtc,
        Questions = session.Answers.Select(ToPracticeQuestionDto).ToList()
    };

    public static PracticeSessionResultDto ToResultDto(this PracticeSession session)
    {
        var totalQuestionCount = session.Answers.Count;
        var correctAnswerCount = session.Answers.Count(answer => answer.IsCorrect);

        return new PracticeSessionResultDto
        {
            Id = session.Id,
            TotalQuestions = totalQuestionCount,
            CorrectAnswers = correctAnswerCount,
            ScorePercentage = totalQuestionCount == 0
                ? 0
                : Math.Round(correctAnswerCount * 100.0 / totalQuestionCount, 2),
            Answers = session.Answers.Select(ToAnswerReviewDto).ToList()
        };
    }

    public static AnswerResultDto ToAnswerResultDto(this StudentAnswer answer) => new()
    {
        QuestionId = answer.QuestionId,
        IsCorrect = answer.IsCorrect,
        CorrectOptionId = answer.Question.CorrectOption.Id,
        Justification = answer.Question.Justification,
        JustificationImageUrl = answer.Question.JustificationImageUrl
    };

    private static PracticeQuestionDto ToPracticeQuestionDto(StudentAnswer answer) => new()
    {
        QuestionId = answer.QuestionId,
        Text = answer.Question.Text,
        ImageUrl = answer.Question.ImageUrl,
        Options = answer.Question.Options
            .Select(option => new PracticeQuestionOptionDto
            {
                Id = option.Id,
                Text = option.Text,
                ImageUrl = option.ImageUrl
            })
            .ToList(),
        Answered = answer.IsAnswered,
        SelectedOptionId = answer.SelectedOptionId
    };

    private static AnswerReviewDto ToAnswerReviewDto(StudentAnswer answer) => new()
    {
        QuestionId = answer.QuestionId,
        QuestionText = answer.Question.Text,
        QuestionImageUrl = answer.Question.ImageUrl,
        SelectedOptionId = answer.SelectedOptionId,
        CorrectOptionId = answer.Question.CorrectOption.Id,
        IsCorrect = answer.IsCorrect,
        Justification = answer.Question.Justification,
        JustificationImageUrl = answer.Question.JustificationImageUrl
    };
}
