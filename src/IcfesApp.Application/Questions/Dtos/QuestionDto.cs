using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Questions.Dtos;

/// <summary>
/// Pregunta completa, incluida la opción correcta. Solo la ven profesores y administradores;
/// el estudiante ve las preguntas a través de la sesión de práctica (PracticeQuestionDto).
/// </summary>
public class QuestionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }
    public required string Justification { get; set; }
    public string? JustificationImageUrl { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public Guid SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = [];
}
